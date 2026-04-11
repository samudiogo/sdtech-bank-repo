using FluentAssertions;
using Moq;
using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Application.Payments.Resolvers;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Tests.Payments;

public class ReceiverResolverChainTests
{
    [Fact]
    public async Task Should_Stop_On_First_Resolver_That_Returns_Value()
    {
        //arrange
        var expectedReceiverId = Guid.NewGuid();
        var payment = PaymentOrder.Create(
            new IdempotencyKey(Guid.NewGuid().ToString()),
            Guid.NewGuid(),
            PaymentDestination.FromPixKey("chave"),
            new Money(100, CurrencyType.BRL)
        );

        var firstResolver = new Mock<IReceiverResolutionStep>();
        firstResolver.Setup(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync(expectedReceiverId);

        var secondResolver = new Mock<IReceiverResolutionStep>();
        secondResolver.Setup(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

        var chain = new ReceiverResolverChain([firstResolver.Object, secondResolver.Object]);

        //act

        var result = await chain.ResolveAsync(payment, CancellationToken.None);

        //assert

        result.Should().Be(expectedReceiverId);
        firstResolver.Verify(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
        secondResolver.Verify(r => r.ResolveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact]
    public async Task Should_Call_Next_Resolver_When_Previous_Returns_Null()
    {
        //arrange
        var expectedReceiverId = Guid.NewGuid();
        var payment = PaymentOrder.Create(
            new IdempotencyKey(Guid.NewGuid().ToString()),
            Guid.NewGuid(),
            PaymentDestination.FromPixKey("chave"),
            new Money(100, CurrencyType.BRL)
        );

        var firstResolver = new Mock<IReceiverResolutionStep>();
        firstResolver.Setup(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);

        var secondResolver = new Mock<IReceiverResolutionStep>();
        secondResolver.Setup(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync(expectedReceiverId);

        var chain = new ReceiverResolverChain([firstResolver.Object, secondResolver.Object]);

        //act

        var result = await chain.ResolveAsync(payment, CancellationToken.None);

        //assert

        result.Should().Be(expectedReceiverId);
        firstResolver.Verify(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
        secondResolver.Verify(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Return_Null_When_No_Resolver_Can_Handle()
    {
        //arrange
        Guid? expectedReceiverId = null;
        var payment = PaymentOrder.Create(
            new IdempotencyKey(Guid.NewGuid().ToString()),
            Guid.NewGuid(),
            PaymentDestination.FromPixKey("chave"),
            new Money(100, CurrencyType.BRL)
        );

        var firstResolver = new Mock<IReceiverResolutionStep>();
        firstResolver.Setup(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync(expectedReceiverId);

        var secondResolver = new Mock<IReceiverResolutionStep>();
        secondResolver.Setup(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync(expectedReceiverId);

        var chain = new ReceiverResolverChain([firstResolver.Object, secondResolver.Object]);

        //act

        var result = await chain.ResolveAsync(payment, CancellationToken.None);

        //assert

        result.Should().BeNull();
        firstResolver.Verify(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
        secondResolver.Verify(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }
}
