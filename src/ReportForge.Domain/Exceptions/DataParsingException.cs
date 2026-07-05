namespace ReportForge.Domain.Exceptions;

public sealed class DataParsingException : Exception
{
    public DataParsingException(string message) : base(message) { }

    public DataParsingException(string message, Exception innerException) : base(message, innerException) { }
}
