using SdtechBank.Application.Abstractions.Resilience;

namespace SdtechBank.Infrastructure.Resilience;

public sealed class RetryPolicy : IRetryPolicy
{
    private const int MaxAttempts = 3;
    public bool ShouldRetry(ErrorCategory errorCategory, int attempt)
    {
        if (errorCategory == ErrorCategory.Business)
            return false;

        return attempt < MaxAttempts;
    }

    public TimeSpan GetDelay(int attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt));
}