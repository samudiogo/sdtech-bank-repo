using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.UseCases.ValidatePayment;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Tests.Payments;

public class ValidatePaymentUseCaseTests
{
    private readonly Mock<IPaymentOrderRepository> _repositoryMock;
    private readonly Mock<IOutboxService> _outboxServiceMock;
    private readonly Mock<ILogger<ValidatePaymentUseCase>> _logger;
    public ValidatePaymentUseCaseTests()
    {
        _repositoryMock = new Mock<IPaymentOrderRepository>();
        _outboxServiceMock = new Mock<IOutboxService>();
        _logger = new Mock<ILogger<ValidatePaymentUseCase>>();
    }

    [Fact]
    public async Task Should_Resolve_Receiver_And_Send_PaymentValidated_When_Destination_Has_BankAccount()
    {
        //arrange:
        var payment = PaymentOrder.Create(new IdempotencyKey(Guid.NewGuid().ToString()),Guid.NewGuid(), PaymentDestination.FromBankAccount(new()
        {
            FullName = "Samuel",
            Document = "00012345680",
            BankCode = "001",
            Branch = "1234",
            Account = "123456"
        }), new Money(100, CurrencyType.BRL));

        _repositoryMock.Setup(r => r.GetByIdAsync(payment.Id))
                   .ReturnsAsync(payment);

        var receiverId = Guid.NewGuid();

        var receiverResolverMock = new Mock<IReceiverResolver>();
        receiverResolverMock.Setup(r => r.ResolveAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync(receiverId);

        var useCase = new ValidatePaymentUseCase(_repositoryMock.Object, _outboxServiceMock.Object, receiverResolverMock.Object, _logger.Object);

        //act

        await useCase.ExecuteAsync(payment.Id, CancellationToken.None);

        //assert

        payment.PaymentStatus.Should().Be(PaymentStatus.READY_TO_TRANSFER);

        _outboxServiceMock.Verify(x => x.AddEventAsync(It.IsAny<PaymentValidatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Send_To_WaitingDict_When_Destination_Is_Pix()
    {
        // Arrange
        var payment = PaymentOrder.Create(
            new IdempotencyKey(Guid.NewGuid().ToString()),
            Guid.NewGuid(),
            PaymentDestination.FromPixKey("chave-pix"),
            new Money(100, CurrencyType.BRL)
        );

        _repositoryMock.Setup(r => r.GetByIdAsync(payment.Id)).ReturnsAsync(payment);

        var receiverResolverMock = new Mock<IReceiverResolver>();

        var useCase = new ValidatePaymentUseCase(_repositoryMock.Object, _outboxServiceMock.Object, receiverResolverMock.Object, _logger.Object);

        //act:
        await useCase.ExecuteAsync(payment.Id, CancellationToken.None);

        //assert:
        payment.PaymentStatus.Should().Be(PaymentStatus.WAITING_FOR_DICT);

        receiverResolverMock.Verify(x => x.ResolveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact]
    public async Task Should_Mark_As_Failed_When_Receiver_Cannot_Be_Resolved()
    {
        //arrange
        var payment = PaymentOrder.Create(
                                        new IdempotencyKey(Guid.NewGuid().ToString()),
                                        Guid.NewGuid(),
                                        PaymentDestination.FromBankAccount(new BankAccount
                                        {
                                            FullName = "Samuel",
                                            Document = "00012345680",
                                            BankCode = "001",
                                            Branch = "1234",
                                            Account = "123456"
                                        }),
                                        new Money(100, CurrencyType.BRL));

        _repositoryMock.Setup(r => r.GetByIdAsync(payment.Id))
                  .ReturnsAsync(payment);

        Guid? receiverId = null;

        var receiverResolverMock = new Mock<IReceiverResolver>();
        receiverResolverMock.Setup(r => r.ResolveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>())).ReturnsAsync(receiverId);

        var useCase = new ValidatePaymentUseCase(_repositoryMock.Object, _outboxServiceMock.Object, receiverResolverMock.Object, _logger.Object);

        //act:
        await useCase.ExecuteAsync(payment.Id, CancellationToken.None);

        //assert:
        payment.PaymentStatus.Should().Be(PaymentStatus.FAILED);
    }
}