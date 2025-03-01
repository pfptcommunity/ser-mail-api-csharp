namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Defines a contract for an HTTP client capable of performing common HTTP operations asynchronously.
    /// </summary>
    internal interface IHttpClient
    {
        /// <summary>
        /// Sends an asynchronous GET request to the specified URI.
        /// </summary>
        /// <param name="requestUri">The URI of the resource to retrieve.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the GET request.</returns>
        Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an asynchronous POST request to the specified URI with the provided content.
        /// </summary>
        /// <param name="requestUri">The URI to send the POST request to.</param>
        /// <param name="content">The HTTP content to send in the request body.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the POST request.</returns>
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an asynchronous PUT request to the specified URI with the provided content.
        /// </summary>
        /// <param name="requestUri">The URI to send the PUT request to.</param>
        /// <param name="content">The HTTP content to send in the request body.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the PUT request.</returns>
        Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an asynchronous DELETE request to the specified URI.
        /// </summary>
        /// <param name="requestUri">The URI of the resource to delete.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the DELETE request.</returns>
        Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an asynchronous HTTP request using a fully configured <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. Defaults to none.</param>
        /// <returns>A task that resolves to the <see cref="HttpResponseMessage"/> from the request.</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}