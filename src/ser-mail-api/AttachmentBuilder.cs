using Proofpoint.SecureEmailRelay.Mail;

public class AttachmentBuilder
{
    private string _content = null!;
    private string _filename = null!;
    private string _mimeType = null!;
    private Disposition _disposition = Disposition.Attachment;
    private string _Id = null!;
    private bool _hasSource = false;
    private readonly IMimeTypeMapper _mimeTypeMapper;

    public AttachmentBuilder() : this(new DefaultMimeTypeMapper()) { }

    public AttachmentBuilder(IMimeTypeMapper mimeTypeMapper)
    {
        _mimeTypeMapper = mimeTypeMapper ?? throw new ArgumentNullException(nameof(mimeTypeMapper));
    }


    public AttachmentBuilder SetDisposition(Disposition disposition)
    {
        _disposition = disposition;

        if (_disposition == Disposition.Inline && string.IsNullOrWhiteSpace(_Id))
        {
            _Id = Guid.NewGuid().ToString();
        }
        return this;
    }

    public AttachmentBuilder FromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        _content = EncodeFileContent(filePath);
        _filename = Path.GetFileName(filePath);
        _mimeType = _mimeTypeMapper.GetMimeType(_filename);
        _hasSource = true;
        return this;
    }

    public AttachmentBuilder FromBase64(string base64Content)
    {
        base64Content = base64Content.Trim();

        if (base64Content is null)
            throw new ArgumentNullException(nameof(base64Content), "Base64 content cannot be null.");

        if (!TryDecodeBase64(base64Content, out _))
            throw new ArgumentException("Invalid Base64 content", nameof(base64Content));

        _content = base64Content;
        _hasSource = true;
        return this;
    }

    public AttachmentBuilder FromBytes(byte[] data)
    {
        if (data == null)
            throw new ArgumentException("Byte array cannot be null.", nameof(data));

        //TODO: check if ToBase64String will work if data is an empty array.
        _content = data.Length > 0 ? Convert.ToBase64String(data) : string.Empty;
        _hasSource = true;
        return this;
    }

    public AttachmentBuilder SetFilename(string filename)
    {
        if (!_hasSource)
            throw new InvalidOperationException("You must set the content source before setting the filename.");

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be empty or whitespace.", nameof(filename));

        _filename = filename;
        return this;
    }

    public AttachmentBuilder SetMimeType(string mimeType)
    {
        if (!_hasSource)
            throw new InvalidOperationException("You must set the content source before setting the MIME type.");

        if (string.IsNullOrWhiteSpace(mimeType) || !_mimeTypeMapper.IsValidMimeType(mimeType))
            throw new ArgumentException($"Invalid MIME type: {mimeType}", nameof(mimeType));

        _mimeType = mimeType;
        return this;
    }

    public AttachmentBuilder SetContentId(string contentId)
    {
        if (_disposition != Disposition.Inline)
            throw new InvalidOperationException("Content ID can only be set for inline attachments.");

        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentException("Content ID cannot be empty or whitespace.", nameof(contentId));

        _Id = contentId;
        return this;
    }

    public Attachment Build()
    {
        Validate();
        return new Attachment(_content, _filename, _mimeType, _disposition, _Id);
    }

    private void Validate()
    {
        if (!_hasSource)
            throw new InvalidOperationException("You must set the content source before building the attachment.");

        if (string.IsNullOrWhiteSpace(_filename))
            throw new InvalidOperationException("Filename must be set.");

        if (string.IsNullOrWhiteSpace(_mimeType))
            throw new InvalidOperationException("MIME type must be set.");

        if (_disposition == Disposition.Inline && string.IsNullOrWhiteSpace(_Id))
            throw new InvalidOperationException("Inline attachments must have a Content ID.");
    }

    private static string EncodeFileContent(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);

        if (fileBytes.Length == 0)
            throw new ArgumentException($"File '{filePath}' is empty and cannot be converted to an attachment.");

        return Convert.ToBase64String(fileBytes);
    }

    private static bool TryDecodeBase64(string base64String, out byte[]? decodedBytes)
    {
        if (base64String is null)
        {
            decodedBytes = Array.Empty<byte>();
            return false;
        }

        try
        {
            decodedBytes = Convert.FromBase64String(base64String);
            return true;
        }
        catch (FormatException)
        {
            decodedBytes = null;
            return false;
        }
    }
}
