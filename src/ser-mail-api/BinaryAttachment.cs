namespace ser_mail_api
{
    public class BinaryAttachment : Attachment
    {
        public BinaryAttachment(byte[] bytes, string filename, string mimeType, Disposition disposition = Disposition.Attachment, bool validateMimeType = true)
            : base(EncodeBytes(bytes), filename, mimeType, disposition, validateMimeType)
        { }

        private static string EncodeBytes(byte[] bytes)
        {
            if (bytes?.Length == 0)
                throw new ArgumentException("Bytes cannot be null or empty.", nameof(bytes));

            return Convert.ToBase64String(bytes);
        }
    }
}
