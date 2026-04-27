using FluentAssertions;
using Moq;
using SdtechBank.Application.Payments.Resolvers;
using SdtechBank.Application.Payments.Resolvers.Specifications;
using SdtechBank.Domain.Accounts;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Tests.Payments;

public class ReceiverResolverTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new();

    [Fact]
    public async Task ResolveAsync_WhenInternalBank_ShouldReturnReceiverId()
    {
        // Arrange
        var receiverId = Guid.NewGuid();

        var payment = PaymentOrder.Create(
            key: new IdempotencyKey(Guid.NewGuid().ToString()),
            payerId: Guid.NewGuid(),
            amount: new Money(100, CurrencyType.BRL),
            destination: PaymentDestination.FromBankAccount(new BankAccount
            {
                FullName = "Samuel",
                Document = "12345678901",
                BankCode = "999",
                Branch = "0001",
                Account = "12345-6"
            }));

        _accountRepositoryMock.Setup(x => x.GetByBankAccountAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Account { Id = receiverId });

        var sut = CreateResolver();

        // Act
        var result = await sut.ResolveAsync(payment, CancellationToken.None);

        // Assert
        result.Should().Be(receiverId);

        _accountRepositoryMock.Verify(x => x.GetByBankAccountAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>()), Times.Once);

        _accountRepositoryMock.Verify(x => x.GetByAccountCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAsync_WhenExternalBank_ShouldReturnReceiverId()
    {
        // Arrange
        var receiverId = Guid.NewGuid();

        var payment = PaymentOrder.Create(
            key: new IdempotencyKey(Guid.NewGuid().ToString()),
            payerId: Guid.NewGuid(),
            amount: new Money(100, CurrencyType.BRL),
            destination: PaymentDestination.FromBankAccount(new BankAccount
            {
                FullName = "Samuel",
                Document = "12345678901",
                BankCode = "001",
                Branch = "0001",
                Account = "12345-6"
            }));

        _accountRepositoryMock
            .Setup(x => x.GetByAccountCodeAsync("00001-1", It.IsAny<CancellationToken>())).ReturnsAsync(new Account { Id = receiverId });

        var sut = CreateResolver();

        // Act
        var result = await sut.ResolveAsync(payment, CancellationToken.None);

        // Assert
        result.Should().Be(receiverId);

        _accountRepositoryMock.Verify(x => x.GetByAccountCodeAsync("00001-1", It.IsAny<CancellationToken>()), Times.Once);

        _accountRepositoryMock.Verify(x => x.GetByBankAccountAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAsync_WhenNoStrategyMatches_ShouldReturnNull()
    {
        // Arrange
        var payment = PaymentOrder.Create(
            key: new IdempotencyKey(Guid.NewGuid().ToString()),
            payerId: Guid.NewGuid(),
            amount: new Money(100, CurrencyType.BRL),
            destination: PaymentDestination.FromPixKey("samuel@email.com"));

        var sut = CreateResolver();

        // Act
        var result = await sut.ResolveAsync(payment, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_WhenStrategyMatchesButAccountNotFound_ShouldReturnNull()
    {
        // Arrange
        var payment = PaymentOrder.Create(
            key: new IdempotencyKey(Guid.NewGuid().ToString()),
            payerId: Guid.NewGuid(),
            amount: new Money(100, CurrencyType.BRL),
            destination: PaymentDestination.FromBankAccount(new BankAccount
            {
                FullName = "Samuel",
                Document = "12345678901",
                BankCode = "999",
                Branch = "0001",
                Account = "12345-6"
            }));

        _accountRepositoryMock
            .Setup(x => x.GetByBankAccountAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var sut = CreateResolver();

        // Act
        var result = await sut.ResolveAsync(payment, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    private ReceiverResolver CreateResolver()
    {
        var strategies = new IReceiverStrategy[]
        {
            new InternalAccountReceiverStrategy(
                _accountRepositoryMock.Object,
                new InternalBankSpecification()),

            new ExternalAccountReceiverStrategy(
                _accountRepositoryMock.Object,
                new ExternalBankSpecification())
        };

        return new ReceiverResolver(strategies);
    }
}