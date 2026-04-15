using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SdtechBank.Application.Accounts.Contracts;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Transactions.Contracts.Events;
using SdtechBank.Application.Transactions.Exceptions;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.Ledger.Enums;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Domain.Transactions.Entities;
using SdtechBank.Domain.Transactions.Enums;

namespace SdtechBank.Application.Tests.Transactions;

public class ProcessPaymentCreatedUseCaseTests
{
    private readonly Mock<ITransactionRepository> transactionRepository;
    private readonly Mock<ILedgerRepository> ledgerRepository;
    private readonly Mock<IAccountBalanceService> balanceService;
    private readonly Mock<IAccountLockService> lockService;
    private readonly Mock<IOutboxService> outboxService;
    private readonly Mock<ILogger<ProcessPaymentCreatedUseCase>> logger;
    private readonly Mock<IDisposable> lockHandle;

    private readonly ProcessPaymentCreatedUseCase sut;

    // Fixtures compartilhados
    private readonly Guid paymentId = Guid.NewGuid();
    private readonly Guid payerId = Guid.NewGuid();
    private readonly Guid receiverId = Guid.NewGuid();
    private readonly Money amount = new(150m, CurrencyType.BRL);
    private readonly string idempotencyKey = "idem-key-001";

    public ProcessPaymentCreatedUseCaseTests()
    {
        transactionRepository = new Mock<ITransactionRepository>();
        ledgerRepository = new Mock<ILedgerRepository>();
        balanceService = new Mock<IAccountBalanceService>();
        lockService = new Mock<IAccountLockService>();
        outboxService = new Mock<IOutboxService>();
        logger = new Mock<ILogger<ProcessPaymentCreatedUseCase>>();
        lockHandle = new Mock<IDisposable>();

        // Lock sempre adquirido com sucesso por padrão
        lockService
            .Setup(x => x.AcquireLockAsync(It.IsAny<Guid>()))
            .ReturnsAsync(lockHandle.Object);

        sut = new ProcessPaymentCreatedUseCase(
            transactionRepository.Object,
            ledgerRepository.Object,
            balanceService.Object,
            lockService.Object,
            outboxService.Object,
            logger.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // TC1 · Idempotência
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenTransactionAlreadyExists_ShouldReturnImmediately()
    {
        // Arrange
        var existingTransaction = Transaction.Create(paymentId, idempotencyKey);

        transactionRepository
            .Setup(x => x.GetByIdempotencyKeyAsync(idempotencyKey))
            .ReturnsAsync(existingTransaction);

        // Act
        await sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert
        transactionRepository.Verify(x => x.SaveAsync(It.IsAny<Transaction>()), Times.Never);
        ledgerRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<LedgerEntry>>()), Times.Never);
        outboxService.Verify(x => x.AddEventAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    // TC2 · Caminho feliz
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenBalanceSufficient_ShouldSaveCompletedTransaction()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeSufficientBalance(200m);

        // Act
        await sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert — SaveAsync chamado duas vezes: StartProcessing e MarkAsCompleted
        transactionRepository.Verify(x => x.SaveAsync(It.Is<Transaction>(t =>
            t.Status == TransactionStatus.COMPLETED)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceSufficient_ShouldSaveDebitAndCreditLedgerEntries()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeSufficientBalance(200m);

        // Act
        await sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert — um débito (payer) e um crédito (receiver)
        ledgerRepository.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<LedgerEntry>>(entries =>
            entries.Count() == 2 &&
            entries.Any(e => e.AccountId == payerId && e.Type == LedgerEntryType.DEBIT) &&
            entries.Any(e => e.AccountId == receiverId && e.Type == LedgerEntryType.CREDIT)
        )), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceSufficient_ShouldPublishTransactionCompletedEvent()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeSufficientBalance(200m);

        // Act
        await sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert
        outboxService.Verify(x => x.AddEventAsync(
            It.Is<TransactionCompletedIntegrationEvent>(e =>
                e.PaymentId == paymentId &&
                e.Amount == amount.Value &&
                e.CorrelationId == idempotencyKey),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    // TC3 · Saldo insuficiente
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenBalanceInsufficient_ShouldThrowInsufficientFundsException()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);

        // Act
        var act = () => sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InsufficientFundsException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceInsufficient_ShouldSaveTransactionAsFailed()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);

        // Act
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None));

        // Assert
        transactionRepository.Verify(x => x.SaveAsync(It.Is<Transaction>(t =>
            t.Status == TransactionStatus.FAILED)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceInsufficient_ShouldPublishTransactionFailedEvent()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);

        // Act
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None));

        // Assert
        outboxService.Verify(x => x.AddEventAsync(
            It.Is<TransactionFailedIntegrationEvent>(e =>
                e.PaymentId == paymentId &&
                e.CorrelationId == idempotencyKey &&
                !string.IsNullOrEmpty(e.Reason)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceInsufficient_ShouldNotSaveAnyLedgerEntries()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);

        // Act
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None));

        // Assert — nenhuma entrada de ledger pode ser gravada se o saldo falhou
        ledgerRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<LedgerEntry>>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    // TC4 · Rethrow de exceção genérica
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenUnexpectedExceptionOccurs_ShouldRethrowOriginalException()
    {
        // Arrange
        ArrangeNoExistingTransaction();

        var unexpectedException = new InvalidOperationException("DB fora do ar");
        balanceService
            .Setup(x => x.GetBalanceAsync(payerId))
            .ThrowsAsync(unexpectedException);

        // Act
        var act = () => sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert — exceção original preservada (não swallowed pelo catch interno)
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB fora do ar");
    }

    // ─────────────────────────────────────────────────────────────
    // TC5 · Inner exception ao marcar como Failed
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenMarkingAsFailedThrows_ShouldStillRethrowOriginalException()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);

        // SaveAsync falha quando tentamos salvar a transação como Failed
        transactionRepository
            .Setup(x => x.SaveAsync(It.Is<Transaction>(t => t.Status == TransactionStatus.FAILED)))
            .ThrowsAsync(new Exception("Erro ao persistir Failed"));

        // Act
        var act = () => sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert — exceção original (InsufficientFunds) ainda propagada
        await act.Should().ThrowAsync<InsufficientFundsException>();
    }

    // ─────────────────────────────────────────────────────────────
    // TC6 · Lock
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ShouldAcquireLock_WithCorrectPayerId()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeSufficientBalance(200m);

        // Act
        await sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert
        lockService.Verify(x => x.AcquireLockAsync(payerId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDisposeLock_EvenWhenExceptionIsThrown()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);

        // Act
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            sut.ExecuteAsync(paymentId, payerId, receiverId, amount, idempotencyKey, CancellationToken.None));

        // Assert — using garante o Dispose independente de exceção
        lockHandle.Verify(x => x.Dispose(), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers privados
    // ─────────────────────────────────────────────────────────────

    private void ArrangeNoExistingTransaction()
        => transactionRepository
            .Setup(x => x.GetByIdempotencyKeyAsync(idempotencyKey))
            .ReturnsAsync((Transaction?)null);

    private void ArrangeSufficientBalance(decimal balanceValue)
        => balanceService
            .Setup(x => x.GetBalanceAsync(payerId))
            .ReturnsAsync(new Money(balanceValue, CurrencyType.BRL));

    private void ArrangeInsufficientBalance(decimal balanceValue)
        => balanceService
            .Setup(x => x.GetBalanceAsync(payerId))
            .ReturnsAsync(new Money(balanceValue, CurrencyType.BRL));
}