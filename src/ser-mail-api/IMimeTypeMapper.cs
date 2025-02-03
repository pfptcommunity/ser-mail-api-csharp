namespace Proofpoint.SecureEmailRelay.Mail
{
    public interface IMimeTypeMapper
    {
        public string GetMimeType(string fileName);
        public bool IsValidMimeType(string mimeType);
    }
}
