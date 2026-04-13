namespace SdtechBank.Domain.Shared.Exceptions;

public class DuplicateKeyException : Exception
{
    public DuplicateKeyException(string message) : base(message) { }
    public DuplicateKeyException(string message, Exception innerExcpetion) : base(message, innerExcpetion) { }
}