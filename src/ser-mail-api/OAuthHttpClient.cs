using System.Net.Http.Headers;
using System.Text.Json;

namespace Proofpoint.SecureEmailRelay.Mail
{
    internal class OAuthHttpClient : IHttpClient
    {
        public HttpClient HttpClient { get; }
        private readonly string _tokenEndpoint;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scope;

        private string? _accessToken;
        private DateTime? _tokenExpiration;
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        public OAuthHttpClient(string tokenEndpoint, string clientId, string clientSecret, string scope)
            : this(tokenEndpoint, clientId, clientSecret, scope, new HttpClient()) { }

        public OAuthHttpClient(string tokenEndpoint, string clientId, string clientSecret, string scope, HttpClient httpClient)
        {
            _tokenEndpoint = tokenEndpoint ?? throw new ArgumentNullException(nameof(tokenEndpoint));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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
            using var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "scope", _scope }
                })
            };

            using var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"OAuth token request failed with status {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            _accessToken = root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("OAuth token response did not contain an access token.");

            if (root.TryGetProperty("expires_in", out var expiresIn))
            {
                _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn.GetInt32() - 60);
            }
            else
            {
                throw new InvalidOperationException("OAuth token response is missing expiration details.");
            }
        }

        private void SetAuthorizationHeader()
        {
            if (string.IsNullOrEmpty(_accessToken))
                throw new InvalidOperationException("Access token is missing. Ensure authentication before making a request.");

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            SetAuthorizationHeader();
            return await HttpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            SetAuthorizationHeader();
            return await HttpClient.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            SetAuthorizationHeader();
            return await HttpClient.PutAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            SetAuthorizationHeader();
            return await HttpClient.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
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