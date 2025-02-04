using Proofpoint.SecureEmailRelay.Mail;




public interface ISourceStep
{
    IFileStep File(string filePath);
    IManualStep Base64(string base64Content);
    IManualStep Bytes(byte[] data);
}

// Set any and terminate
public interface IFileStep : IBuildStep
{
    IFileStep Mime(string mimeType);
    IFileStep Name(string filename);
    IFileStep Inline();
    IFileStep Attachment();
}

public interface IManualStep
{
    IManualMimeStep Name(string filename);
}

public interface IManualMimeStep
{
    IManualDispositionStep Mime(string mimeType);
}

public interface IManualDispositionStep : IBuildStep
{
    IInlineStep Inline();
}

public interface IInlineStep
{
    IBuildStep CId(string contentId);
}
public interface IBuildStep
{
    Attachment Build();
}

public class AttachmentBuilder : ISourceStep, IFileStep, IManualStep, IManualMimeStep, IManualDispositionStep, IInlineStep, IBuildStep
{
    private string _content = string.Empty;
    private string _filename = string.Empty;
    private string _mimeType = string.Empty;
    private Disposition _disposition = Disposition.Attachment;
    private string? _id;
    private readonly IMimeTypeMapper _mimeTypeMapper;

    internal AttachmentBuilder(IMimeTypeMapper mimeTypeMapper)
    {
        _mimeTypeMapper = mimeTypeMapper;
    }

    internal AttachmentBuilder() :
        this(new DefaultMimeTypeMapper())
    { }

    // === Step 1: Select source ===
    IFileStep ISourceStep.File(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        _content = Convert.ToBase64String(File.ReadAllBytes(filePath));
        _filename = Path.GetFileName(filePath);
        _mimeType = _mimeTypeMapper.GetMimeType(_filename);

        return this;
    }

    IManualStep ISourceStep.Base64(string base64Content)
    {
        base64Content = base64Content.Trim();

        if (base64Content is null)
            throw new ArgumentNullException(nameof(base64Content), "Base64 content cannot be null.");

        if (!TryDecodeBase64(base64Content, out _))
            throw new ArgumentException("Invalid Base64 content", nameof(base64Content));

        _content = base64Content;
        return this;
    }

    IManualStep ISourceStep.Bytes(byte[] data)
    {
        if (data == null)
            throw new ArgumentException("Byte array cannot be null.", nameof(data));

        _content = data.Length > 0 ? Convert.ToBase64String(data) : string.Empty;
        return this;
    }

    // === Step 2A: File-based methods ===
    public IFileStep Mime(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType) || !_mimeTypeMapper.IsValidMimeType(mimeType))
            throw new ArgumentException($"Invalid MIME type: {mimeType}", nameof(mimeType));

        _mimeType = mimeType;
        return this;
    }

    public IFileStep Name(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be empty.", nameof(filename));

        _filename = filename;
        return this;
    }

    public IFileStep Inline()
    {
        _disposition = Disposition.Inline;
        _id = Guid.NewGuid().ToString();
        return this;
    }

    public IFileStep Attachment()
    {
        _disposition = Disposition.Attachment;
        return this;
    }

    // === Step 2B: Manual-based methods ===
    IManualMimeStep IManualStep.Name(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be empty.", nameof(filename));

        _filename = filename;
        return this;
    }

    IManualDispositionStep IManualMimeStep.Mime(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType) || !_mimeTypeMapper.IsValidMimeType(mimeType))
            throw new ArgumentException($"Invalid MIME type: {mimeType}", nameof(mimeType));

        _mimeType = mimeType;
        return this;
    }

    IInlineStep IManualDispositionStep.Inline()
    {
        _disposition = Disposition.Inline;
        _id = Guid.NewGuid().ToString();
        return this;
    }

    IBuildStep IInlineStep.CId(string contentId)
    {
        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentException("Content ID cannot be empty.", nameof(contentId));

        _id = contentId;
        return this;
    }

    // === Final Step: Build ===
    public Attachment Build()
    {
        if (string.IsNullOrWhiteSpace(_filename))
            throw new InvalidOperationException("Filename must be set.");

        if (string.IsNullOrWhiteSpace(_mimeType))
            throw new InvalidOperationException("MIME type must be set.");

        if (_disposition == Disposition.Inline && string.IsNullOrWhiteSpace(_id))
            throw new InvalidOperationException("Inline attachments must have a Content ID.");

        return new Attachment(_content, _filename, _mimeType, _disposition, _id);
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