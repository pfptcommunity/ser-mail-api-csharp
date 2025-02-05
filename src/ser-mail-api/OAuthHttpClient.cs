using IdentityModel.Client;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Proofpoint.SecureEmailRelay.Mail
{
    internal class OAuthHttpClient : IHttpClient
    {
        public HttpClient HttpClient { get; }
        protected readonly ClientCredentialsTokenRequest _clientCredentials;
        private string? _accessToken;
        private DateTime? _tokenExpiration;
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        public OAuthHttpClient(ClientCredentialsTokenRequest clientCredentials)
        {
            _clientCredentials = clientCredentials ?? throw new ArgumentNullException(nameof(clientCredentials), "Client credentials must not be null.");
            HttpClient = new HttpClient();
        }

        public OAuthHttpClient(HttpClient httpClient, ClientCredentialsTokenRequest clientCredentials)
        {
            _clientCredentials = clientCredentials ?? throw new ArgumentNullException(nameof(clientCredentials), "Client credentials must not be null.");
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "HttpClient instance must not be null.");
        }

        private async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration.HasValue && DateTime.UtcNow < _tokenExpiration.Value)
            {
                return;
            }

            await _tokenSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (string.IsNullOrEmpty(_accessToken) || !_tokenExpiration.HasValue || DateTime.UtcNow >= _tokenExpiration.Value)
                {
                    await RefreshTokenAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        private async Task RefreshTokenAsync()
        {
            var tokenResponse = await HttpClient.RequestClientCredentialsTokenAsync(_clientCredentials).ConfigureAwait(false);

            if (tokenResponse.IsError)
            {
                throw new HttpRequestException($"OAuth token request failed: {tokenResponse.Error}");
            }

            _accessToken = tokenResponse.AccessToken ?? throw new InvalidOperationException("OAuth token response did not contain an access token.");

            if (tokenResponse.HttpResponse is not null)
            {
                var responseContent = await tokenResponse.HttpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                try
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("token_expires_date_time", out var expiresDateTime))
                    {
                        _tokenExpiration = DateTime.Parse(expiresDateTime.GetString()!, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                    else if (root.TryGetProperty("expires_in", out var expiresIn))
                    {
                        _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn.GetInt32() - 60);
                    }
                    else
                    {
                        throw new InvalidOperationException("OAuth token response is missing expiration details.");
                    }
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException("Error parsing OAuth token expiration details from response.", ex);
                }
            }
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, HttpContent? content = null)
        {
            if (string.IsNullOrWhiteSpace(requestUri))
                throw new ArgumentNullException(nameof(requestUri), "Request URI must not be null, empty, or contain only whitespace.");

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            if (string.IsNullOrEmpty(_accessToken))
                throw new InvalidOperationException("Access token is missing. Ensure authentication before making a request.");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return request;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            var request = CreateRequest(HttpMethod.Get, requestUri);
            return await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            var request = CreateRequest(HttpMethod.Post, requestUri, content);
            return await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            var request = CreateRequest(HttpMethod.Put, requestUri, content);
            return await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            var request = CreateRequest(HttpMethod.Delete, requestUri);
            return await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "HTTP request must not be null.");

            await EnsureTokenAsync().ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
