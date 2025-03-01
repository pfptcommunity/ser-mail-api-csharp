namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Defines a contract for mapping file extensions to MIME types and validating MIME types.
    /// </summary>
    public interface IMimeMapper
    {
        /// <summary>
        /// Resolves the MIME type for a given filename based on its extension.
        /// </summary>
        /// <param name="fileName">The filename to determine the MIME type for.</param>
        /// <returns>The MIME type associated with the file extension.</returns>
        public string GetMimeType(string fileName);

        /// <summary>
        /// Determines whether a given MIME type is valid according to the mapper's known types or policies.
        /// </summary>
        /// <param name="mimeType">The MIME type to validate.</param>
        /// <returns>True if the MIME type is valid; otherwise, false.</returns>
        public bool IsValidMimeType(string mimeType);
    }
}