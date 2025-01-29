namespace Proofpoint.SecureEmailRelay.Mail
{
    public class BinaryAttachment : Attachment
    {
        public BinaryAttachment(byte[] bytes, string filename, string mimeType, Disposition disposition = Disposition.Attachment, bool validateMimeType = true)
            : base(EncodeBytes(bytes), filename, mimeType, disposition, validateMimeType)
        { }

        private static string EncodeBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) // Explicit null check
                throw new ArgumentException("Bytes cannot be null or empty.", nameof(bytes));

            return Convert.ToBase64String(bytes);
        }
    }
}
