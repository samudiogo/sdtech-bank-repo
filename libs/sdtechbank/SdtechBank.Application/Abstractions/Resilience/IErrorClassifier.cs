namespace SdtechBank.Application.Abstractions.Resilience;

public interface IErrorClassifier
{
    ErrorCategory Classify(Exception exception);
}