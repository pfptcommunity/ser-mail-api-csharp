using System.Net.Http.Headers;
using System.Text.Json;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Implements an HTTP client with OAuth authentication for interacting with the Proofpoint Secure Email Relay API.
    /// </summary>
    internal class OAuthHttpClient : IHttpClient
    {
        /// <summary>
        /// The time offset in seconds before token expiration to trigger a refresh.
        /// </summary>
        private readonly int _tokenRefreshOffset;

        /// <summary>
        /// Gets the underlying HTTP client used for making requests.
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// The endpoint URI for obtaining OAuth tokens.
        /// </summary>
        private readonly string _tokenEndpoint;

        /// <summary>
        /// The client ID for OAuth authentication.
        /// </summary>
        private readonly string _clientId;

        /// <summary>
        /// The client secret for OAuth authentication.
        /// </summary>
        private readonly string _clientSecret;

        /// <summary>
        /// The scope requested for the OAuth token.
        /// </summary>
        private readonly string _scope;

        /// <summary>
        /// The current OAuth access token, or null if not yet retrieved.
        /// </summary>
        private string? _accessToken;

        /// <summary>
        /// The expiration time of the current access token, or null if not set.
        /// </summary>
        private DateTime? _tokenExpiration;

        /// <summary>
        /// A semaphore to synchronize token refresh operations.
        /// </summary>
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        /// <summary>
        /// Initializes a new instance of <see cref="OAuthHttpClient"/> with the specified OAuth parameters and a default HTTP client.
        /// </summary>
        /// <param name="tokenEndpoint">The OAuth token endpoint URI.</param>
        /// <param name="clientId">The client ID for authentication.</param>
        /// <param name="clientSecret">The client secret for authentication.</param>
        /// <param name="scope">The requested scope for the token.</param>
        /// <param name="tokenRefreshOffset">The seconds before token expiration to refresh it.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required string parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tokenRefreshOffset"/> is negative.</exception>
        public OAuthHttpClient(string tokenEndpoint, string clientId, string clientSecret, string scope, int tokenRefreshOffset)
            : this(tokenEndpoint, clientId, clientSecret, scope, tokenRefreshOffset, new HttpClient()) { }

        /// <summary>
        /// Initializes a new instance of <see cref="OAuthHttpClient"/> with the specified OAuth parameters and HTTP client.
        /// </summary>
        /// <param name="tokenEndpoint">The OAuth token endpoint URI.</param>
        /// <param name="clientId">The client ID for authentication.</param>
        /// <param name="clientSecret">The client secret for authentication.</param>
        /// <param name="scope">The requested scope for the token.</param>
        /// <param name="tokenRefreshOffset">The seconds before token expiration to refresh it.</param>
        /// <param name="httpClient">The HTTP client instance to use for requests.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tokenRefreshOffset"/> is negative.</exception>
        public OAuthHttpClient(string tokenEndpoint, string clientId, string clientSecret, string scope, int tokenRefreshOffset, HttpClient httpClient)
        {
            _tokenEndpoint = tokenEndpoint ?? throw new ArgumentNullException(nameof(tokenEndpoint));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (tokenRefreshOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(tokenRefreshOffset), "Token refresh offset must be a positive number.");

            _tokenRefreshOffset = tokenRefreshOffset;
        }

        /// <summary>
        /// Ensures a valid OAuth token is available, refreshing it if expired or missing.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Refreshes the OAuth access token by making a request to the token endpoint.
        /// </summary>
        /// <returns>A task representing the asynchronous token refresh operation.</returns>
        /// <exception cref="HttpRequestException">Thrown when the token request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the token response lacks required fields.</exception>
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

            if (root.TryGetProperty("token_expires_date_time", out var expiresDateTime))
            {
                _tokenExpiration = DateTime.Parse(expiresDateTime.GetString()!, null, System.Globalization.DateTimeStyles.RoundtripKind).AddSeconds(-_tokenRefreshOffset);
            }
            else if (root.TryGetProperty("expires_in", out var expiresIn))
            {
                _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn.GetInt32() - _tokenRefreshOffset);
            }
            else
            {
                throw new InvalidOperationException("OAuth token response is missing expiration details.");
            }
        }

        /// <summary>
        /// Sets the Bearer authorization header on the HTTP client using the current access token.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the access token is missing.</exception>
        private void SetAuthorizationHeader()
        {
            if (string.IsNullOrEmpty(_accessToken))
                throw new InvalidOperationException("Access token is missing. Ensure authentication before making a request.");

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        /// <summary>
        /// Sends an asynchronous GET request to the specified URI with OAuth authentication.
        /// </summary>
        /// <param name="requestUri">The URI of the resource to retrieve.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the GET request.</returns>
        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            SetAuthorizationHeader();
            return await HttpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends an asynchronous POST request to the specified URI with OAuth authentication.
        /// </summary>
        /// <param name="requestUri">The URI to send the POST request to.</param>
        /// <param name="content">The HTTP content to send in the request body.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the POST request.</returns>
        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            SetAuthorizationHeader();
            return await HttpClient.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends an asynchronous PUT request to the specified URI with OAuth authentication.
        /// </summary>
        /// <param name="requestUri">The URI to send the PUT request to.</param>
        /// <param name="content">The HTTP content to send in the request body.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the PUT request.</returns>
        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            SetAuthorizationHeader();
            return await HttpClient.PutAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends an asynchronous DELETE request to the specified URI with OAuth authentication.
        /// </summary>
        /// <param name="requestUri">The URI of the resource to delete.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the DELETE request.</returns>
        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync().ConfigureAwait(false);
            SetAuthorizationHeader();
            return await HttpClient.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends an asynchronous HTTP request with OAuth authentication using a fully configured request message.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the request.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
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