using FluentAssertions;
using Moq;
using SdtechBank.Application.DictServices;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Application.Payments.UseCases.ResolvePaymentDictUseCase;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Tests.Payments;

public class ResolvePaymentDictUseCaseTests
{
    private readonly Mock<IPaymentOrderRepository> _repositoryMock = new();
    private readonly Mock<IOutboxService> _outboxMock = new();
    private readonly Mock<IDictClient> _dictClientMock = new();
    private readonly Mock<IReceiverResolver> _receiverResolverMock = new();

    private readonly ResolvePaymentDictUseCase _sut;

    public ResolvePaymentDictUseCaseTests()
    {
        _sut = new ResolvePaymentDictUseCase(
            _repositoryMock.Object,
            _outboxMock.Object,
            _dictClientMock.Object,
            _receiverResolverMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentOrderNotFound_ShouldReturn()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync((PaymentOrder?)null);

        // Act
        await _sut.ExecuteAsync(paymentId, CancellationToken.None);

        // Assert
        _dictClientMock.Verify(
            x => x.GetKeyAsync(It.IsAny<PixKey>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDictNotFound_ShouldMarkAsFailedAndSave()
    {
        // Arrange
        var payment = PaymentOrder.Create(
            key: new IdempotencyKey(Guid.NewGuid().ToString()),
            payerId: Guid.NewGuid(),
            amount: new Money(100, CurrencyType.BRL),
            destination: PaymentDestination.FromPixKey("samuel@email.com"));

        _repositoryMock
            .Setup(x => x.GetByIdAsync(payment.Id))
            .ReturnsAsync(payment);

        _dictClientMock
            .Setup(x => x.GetKeyAsync(It.IsAny<PixKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DictKeyResponse?)null);

        // Act
        await _sut.ExecuteAsync(payment.Id, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.SaveAsync(payment, It.IsAny<CancellationToken>()),
            Times.Once);

        _outboxMock.Verify(
            x => x.AddEventAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenReceiverNotFound_ShouldSaveTwiceAndNotPublishEvent()
    {
        // Arrange
        var payment = PaymentOrder.Create(
            key: new IdempotencyKey(Guid.NewGuid().ToString()),
            payerId: Guid.NewGuid(),
            amount: new Money(100, CurrencyType.BRL),
            destination: PaymentDestination.FromPixKey("samuel@email.com"));

        _repositoryMock
            .Setup(x => x.GetByIdAsync(payment.Id))
            .ReturnsAsync(payment);

        _dictClientMock
            .Setup(x => x.GetKeyAsync(It.IsAny<PixKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDictResponse());

        _receiverResolverMock
            .Setup(x => x.ResolveAsync(payment, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        // Act
        await _sut.ExecuteAsync(payment.Id, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.SaveAsync(payment, It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _outboxMock.Verify(
            x => x.AddEventAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEverythingIsValid_ShouldSaveAndPublishEvent()
    {
        // Arrange
        var payment = PaymentOrder.Create(
            key: new IdempotencyKey(Guid.NewGuid().ToString()),
            payerId: Guid.NewGuid(),
            amount: new Money(100, CurrencyType.BRL),
            destination: PaymentDestination.FromPixKey("samuel@email.com"));

        var receiverId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(payment.Id))
            .ReturnsAsync(payment);

        _dictClientMock
            .Setup(x => x.GetKeyAsync(It.IsAny<PixKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDictResponse());

        _receiverResolverMock
            .Setup(x => x.ResolveAsync(payment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receiverId);

        // Act
        await _sut.ExecuteAsync(payment.Id, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.SaveAsync(payment, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _outboxMock.Verify(x => x.AddEventAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static DictKeyResponse CreateDictResponse()
    {
        return new DictKeyResponse
        {
            Key = "samuel@email.com",
            KeyNormalized = "samuel@email.com",
            KeyType = "Email",
            Status = "ACTIVE",
            Owner = new OwnerResponse
            {
                Name = "Samuel",
                Document = "12345678901",
                DocumentType = "CPF"
            },
            Account = new AccountResponse
            {
                Ispb = "12345678",
                BankCode = "001",
                BankName = "Banco Teste",
                Branch = "0001",
                Number = "12345",
                Digit = "9",
                AccountType = "Checking"
            },
            Metadata = new MetadataResponse
            {
                Version = 1
            }
        };
    }
}