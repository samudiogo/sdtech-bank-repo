using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using SdtechBank.Application.Payments.UseCases.CompletePayment;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Tests.Payments;

public class CompletePaymentUseCaseTests
{
    private readonly Mock<IPaymentOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<CompletePaymentUseCase>> _logger;

    public CompletePaymentUseCaseTests()
    {
        _repositoryMock = new Mock<IPaymentOrderRepository>();
        _logger = new Mock<ILogger<CompletePaymentUseCase>>();
    }

    [Fact]
    public async Task Should_Find_PaymentById_And_MarkAsCompleted()
    {
        //arrange:
        var transactionId = Guid.NewGuid();
        var payment = PaymentOrder.Create(new IdempotencyKey(Guid.NewGuid().ToString()), Guid.NewGuid(), PaymentDestination.FromBankAccount(new()
        {
            FullName = "Samuel",
            Document = "00012345680",
            BankCode = "001",
            Branch = "1234",
            Account = "123456"
        }), new Money(100, CurrencyType.BRL));
        payment.MarkAsWaitingConfirmation();
        payment.MarkAsReadyToTransfer();
        payment.MarkAsInTransfer();

        _repositoryMock.Setup(r => r.GetByIdAsync(payment.Id))
                   .ReturnsAsync(payment);

        var useCase = new CompletePaymentUseCase(_repositoryMock.Object, _logger.Object);

        //act
        await useCase.ExecuteAsync(payment.Id, transactionId, CancellationToken.None);

        //assert

        payment.PaymentStatus.Should().Be(PaymentStatus.COMPLETED);
        payment.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, 1.Hours());
        payment.TransactionId.Should().Be(transactionId);

        _repositoryMock.Verify(x => x.SaveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
    public async Task Should_Not_Find_PaymentOrder_And_Do_Nothing()
    {
        //arrange:
        var transactionId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        PaymentOrder? payment = null;

        _repositoryMock.Setup(r => r.GetByIdAsync(paymentId))
                   .ReturnsAsync(payment);

        var useCase = new CompletePaymentUseCase(_repositoryMock.Object, _logger.Object);

        //act
        await useCase.ExecuteAsync(paymentId, transactionId, CancellationToken.None);

        //assert

        payment.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(paymentId), Times.Once);
        _repositoryMock.Verify(x => x.SaveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()), Times.Never);

    }
}
