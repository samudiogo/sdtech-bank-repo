using SdtechBank.Application.Abstractions.Resilience;
using SdtechBank.Application.Transactions.Exceptions;

namespace SdtechBank.Infrastructure.Resilience;

public sealed class ErrorClassifier : IErrorClassifier
{
    public ErrorCategory Classify(Exception exception)
    {
        return exception switch
        {
            InsufficientFundsException => ErrorCategory.Business,
            TimeoutException => ErrorCategory.Transient,
            TaskCanceledException => ErrorCategory.Transient,

            _ => ErrorCategory.Fatal
        };
    }
}