namespace Proofpoint.SecureEmailRelay.Mail
{
    public interface IMimeMapper
    {
        public string GetMimeType(string fileName);
        public bool IsValidMimeType(string mimeType);
    }
}
