
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using SdtechBank.Application.Payments.UseCases.FailPayment;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Tests.Payments;

public class FailPaymentUseCaseTests
{
    private readonly Mock<IPaymentOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<FailPaymentUseCase>> _logger;

    public FailPaymentUseCaseTests()
    {
        _repositoryMock = new Mock<IPaymentOrderRepository>();
        _logger = new Mock<ILogger<FailPaymentUseCase>>();
    }

    [Fact]
    public async Task Should_Find_PaymentById_And_MarkAsFailed()
    {
        //arrange:
        var reason = "reason";
        var payment = PaymentOrder.Create(new IdempotencyKey(Guid.NewGuid().ToString()), Guid.NewGuid(), PaymentDestination.FromBankAccount(new()
        {
            FullName = "Samuel",
            Document = "00012345680",
            BankCode = "001",
            Branch = "1234",
            Account = "123456"
        }), new Money(100, CurrencyType.BRL));

        _repositoryMock.Setup(r => r.GetByIdAsync(payment.Id))
                   .ReturnsAsync(payment);

        var useCase = new FailPaymentUseCase(_repositoryMock.Object, _logger.Object);

        //act
        await useCase.ExecuteAsync(payment.Id, reason, CancellationToken.None);

        //assert

        payment.PaymentStatus.Should().Be(PaymentStatus.FAILED);
        payment.FailedAt.Should().BeCloseTo(DateTime.UtcNow, 1.Hours());
        payment.FailedReason.Should().Be(reason);

        _repositoryMock.Verify(x => x.SaveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
    public async Task Should_Not_Find_PaymentOrder_And_Do_Nothing()
    {
        //arrange:
        var reason = "reason";
        var paymentId = Guid.NewGuid();
        PaymentOrder? payment = null;

        _repositoryMock.Setup(r => r.GetByIdAsync(paymentId))
                   .ReturnsAsync(payment);

        var useCase = new FailPaymentUseCase(_repositoryMock.Object, _logger.Object);

        //act
        await useCase.ExecuteAsync(paymentId, reason, CancellationToken.None);

        //assert

        payment.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(paymentId), Times.Once);
        _repositoryMock.Verify(x => x.SaveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()), Times.Never);

    }
}
