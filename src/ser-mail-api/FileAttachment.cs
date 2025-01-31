namespace Proofpoint.SecureEmailRelay.Mail
{
    public sealed class FileAttachment : Attachment
    {
        public FileAttachment(string filePath, Disposition disposition = Disposition.Attachment, bool allowFallbackMimeType = false)
            : this(filePath, GetMimeType(filePath, allowFallbackMimeType), disposition)
        {
        }

        public FileAttachment(string filePath, string mimeType, Disposition disposition = Disposition.Attachment, bool validateMimeType = true)
            : base(EncodeFileContent(filePath), Path.GetFileName(filePath), mimeType, disposition)
        { }

        private static string EncodeFileContent(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file at path '{filePath}' does not exist.");

            byte[] fileBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(fileBytes);
        }

        private static string GetMimeType(string filePath, bool allowFallbackMimeType)
        {
            bool isMapped = MimeTypesMap.IsFileMapped(filePath);

            if (!isMapped && !allowFallbackMimeType)
            {
                throw new ArgumentException($"Could not determine MIME type for file: {filePath}. Consider specifying it manually.");
            }

            return MimeTypesMap.GetMimeType(filePath);
        }
    }
}