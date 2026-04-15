using FluentAssertions;
using SdtechBank.Application.Abstractions.Resilience;
using SdtechBank.Infrastructure.Resilience;

namespace SdtechBank.Infrastructure.Tests.Resilience;

public class RetryPolicyTests
{
    private readonly RetryPolicy _sut = new();

    [Fact]
    public void ShouldRetry_WhenErrorCategoryIsBusiness_ShouldReturnFalse()
    {
        var result = _sut.ShouldRetry(ErrorCategory.Business, attempt: 0);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ShouldRetry_WhenErrorCategoryIsBusinessRegardlessOfAttempt_ShouldAlwaysReturnFalse(int attempt)
    {
        var result = _sut.ShouldRetry(ErrorCategory.Business, attempt);

        result.Should().BeFalse();
    }

    // ShouldRetry - Transient
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ShouldRetry_WhenErrorCategoryIsTransientAndAttemptIsUnderMaxAttempts_ShouldReturnTrue(int attempt)
    {
        var result = _sut.ShouldRetry(ErrorCategory.Transient, attempt);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(99)]
    public void ShouldRetry_WhenErrorCategoryIsTransientAndAttemptReachedMaxAttempts_ShouldReturnFalse(int attempt)
    {
        var result = _sut.ShouldRetry(ErrorCategory.Transient, attempt);

        result.Should().BeFalse();
    }

    // ShouldRetry - Fatal
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ShouldRetry_WhenErrorCategoryIsFatalAndAttemptIsUnderMaxAttempts_ShouldReturnTrue(int attempt)
    {
        var result = _sut.ShouldRetry(ErrorCategory.Fatal, attempt);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(99)]
    public void ShouldRetry_WhenErrorCategoryIsFatalAndAttemptReachedMaxAttempts_ShouldReturnFalse(int attempt)
    {
        var result = _sut.ShouldRetry(ErrorCategory.Fatal, attempt);

        result.Should().BeFalse();
    }

    // GetDelay
    [Theory]
    [InlineData(0, 1)]   // 2^0 = 1s
    [InlineData(1, 2)]   // 2^1 = 2s
    [InlineData(2, 4)]   // 2^2 = 4s
    [InlineData(3, 8)]   // 2^3 = 8s
    public void GetDelay_ShouldReturnExponentialBackoff(int attempt, double expectedSeconds)
    {
        var result = _sut.GetDelay(attempt);

        result.Should().Be(TimeSpan.FromSeconds(expectedSeconds));
    }
}