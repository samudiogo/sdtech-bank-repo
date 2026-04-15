using FluentAssertions;
using SdtechBank.Application.Abstractions.Resilience;
using SdtechBank.Application.Transactions.Exceptions;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Infrastructure.Resilience;

namespace SdtechBank.Infrastructure.Tests.Resilience;

public class ErrorClassifierTests
{
    private readonly ErrorClassifier _classifier = new();
    
    [Fact]
    public void Classify_InsufficientFundsException_Should_Classify_As_BussinessErrorType()
    {
        var exception = new InsufficientFundsException(Guid.NewGuid(), new Money(100, CurrencyType.BRL), new Money(10, CurrencyType.BRL));
        var result = _classifier.Classify(exception);

        result.Should().Be(ErrorCategory.Business);
    }

    [Fact]
    public void Classify_TimeoutException_Should_Classify_As_TransientErrorType()
    {
        var exception = new TimeoutException();
        var result = _classifier.Classify(exception);

        result.Should().Be(ErrorCategory.Transient);
    }

    [Fact]
    public void Classify_TaskCanceledException_Should_Classify_As_TransientErrorType()
    {
        var exception = new TaskCanceledException();
        var result = _classifier.Classify(exception);

        result.Should().Be(ErrorCategory.Transient);
    }

    [Fact]
    public void Classify_UnknownException_Should_Classify_As_FatalErrorType()
    {
        var exception = new Exception("unexpected error");
        var result = _classifier.Classify(exception);

        result.Should().Be(ErrorCategory.Fatal);
    }
}