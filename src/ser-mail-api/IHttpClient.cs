namespace Proofpoint.SecureEmailRelay.Mail
{
    internal interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}
