namespace Proofpoint.SecureEmailRelay.Mail
{
    public sealed class BinaryAttachment : Attachment
    {
        public BinaryAttachment(byte[] bytes, string filename, string mimeType, Disposition disposition = Disposition.Attachment, bool validateMimeType = true)
            : base(EncodeBytes(bytes), filename, mimeType, disposition, validateMimeType)
        { }

        private static string EncodeBytes(byte[] bytes)
        {
            if (bytes.Length == 0)
                return string.Empty;

            return Convert.ToBase64String(bytes);
        }
    }
}
