namespace SdtechBank.Application.Abstractions.Resilience;

public interface IRetryPolicy
{
    bool ShouldRetry(ErrorCategory errorCategory, int attempt);
    TimeSpan GetDelay(int attempt);
}