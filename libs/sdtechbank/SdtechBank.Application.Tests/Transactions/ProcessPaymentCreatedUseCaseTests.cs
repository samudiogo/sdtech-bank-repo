using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SdtechBank.Application.Abstractions.Persistence;
using SdtechBank.Application.Accounts.Contracts;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Transactions.Contracts.Events;
using SdtechBank.Application.Transactions.Exceptions;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.Ledger.Enums;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Domain.Transactions.Entities;
using SdtechBank.Domain.Transactions.Enums;

namespace SdtechBank.Application.Tests.Transactions;

public class ProcessPaymentCreatedUseCaseTests
{
    private readonly Mock<ITransactionRepository> _transactionRepository;
    private readonly Mock<IPaymentOrderRepository> _paymentOrderRepository;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILedgerRepository> _ledgerRepository;
    private readonly Mock<IAccountBalanceService> _balanceService;
    private readonly Mock<IAccountLockService> _lockService;
    private readonly Mock<IOutboxService> _outboxService;
    private readonly Mock<ILogger<ProcessPaymentCreatedUseCase>> _logger;
    private readonly Mock<IDisposable> _lockHandle;

    private readonly ProcessPaymentCreatedUseCase _sut;

    // Fixtures compartilhados
    private readonly PaymentOrder ValidPaymentOrder = PaymentOrder.Create(new IdempotencyKey(Guid.NewGuid().ToString()), Guid.NewGuid(), PaymentDestination.FromBankAccount(new()
    {
        FullName = "Samuel",
        Cpf = "00012345680",
        BankCode = "001",
        Branch = "1234",
        Account = "123456"
    }), new Money(100, CurrencyType.BRL));

    private readonly Guid payerId = Guid.NewGuid();
    private readonly Guid receiverId = Guid.NewGuid();
    private readonly Money amount = new(150m, CurrencyType.BRL);
    private readonly string idempotencyKey = "idem-key-001";

    public ProcessPaymentCreatedUseCaseTests()
    {
        _transactionRepository = new Mock<ITransactionRepository>();
        _paymentOrderRepository = new Mock<IPaymentOrderRepository>();
        _ledgerRepository = new Mock<ILedgerRepository>();
        _balanceService = new Mock<IAccountBalanceService>();
        _lockService = new Mock<IAccountLockService>();
        _outboxService = new Mock<IOutboxService>();
        _logger = new Mock<ILogger<ProcessPaymentCreatedUseCase>>();
        _lockHandle = new Mock<IDisposable>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Lock sempre adquirido com sucesso por padrão
        _lockService
            .Setup(x => x.AcquireLockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_lockHandle.Object);

        _sut = new ProcessPaymentCreatedUseCase(
            _unitOfWorkMock.Object,
            _transactionRepository.Object,
            _paymentOrderRepository.Object,
            _ledgerRepository.Object,
            _balanceService.Object,
            _lockService.Object,
            _outboxService.Object,
            _logger.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // TC1 · Idempotência
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenTransactionAlreadyExists_ShouldReturnImmediately()
    {
        // Arrange
        var existingTransaction = Transaction.Create(ValidPaymentOrder.Id, idempotencyKey);

        _transactionRepository
            .Setup(x => x.GetByIdempotencyKeyAsync(idempotencyKey))
            .ReturnsAsync(existingTransaction);

        // Act
        await _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert
        _transactionRepository.Verify(x => x.SaveAsync(It.IsAny<Transaction>()), Times.Never);
        _ledgerRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<LedgerEntry>>()), Times.Never);
        _outboxService.Verify(x => x.AddEventAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
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
        ArrangeExisitingPaymentOrder();

        // Act
        await _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert — SaveAsync chamado duas vezes: StartProcessing e MarkAsCompleted
        _transactionRepository.Verify(x => x.SaveAsync(It.Is<Transaction>(t =>
            t.Status == TransactionStatus.COMPLETED)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceSufficient_ShouldSaveDebitAndCreditLedgerEntries()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeSufficientBalance(200m);
        ArrangeExisitingPaymentOrder();

        // Act
        await _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert — um débito (payer) e um crédito (receiver)
        _ledgerRepository.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<LedgerEntry>>(entries =>
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
        ArrangeExisitingPaymentOrder();

        // Act
        await _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert
        _outboxService.Verify(x => x.AddEventAsync(
            It.Is<TransactionCompletedIntegrationEvent>(e =>
                e.PaymentId == ValidPaymentOrder.Id &&
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
        ArrangeExisitingPaymentOrder();

        // Act
        var act = () => _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InsufficientFundsException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceInsufficient_ShouldSaveTransactionAsFailed()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);
        ArrangeExisitingPaymentOrder();

        // Act
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None));

        // Assert
        _transactionRepository.Verify(x => x.SaveAsync(It.Is<Transaction>(t =>
            t.Status == TransactionStatus.FAILED)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBalanceInsufficient_ShouldPublishTransactionFailedEvent()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);
        ArrangeExisitingPaymentOrder();

        // Act
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None));

        // Assert
        _outboxService.Verify(x => x.AddEventAsync(
            It.Is<TransactionFailedIntegrationEvent>(e =>
                e.PaymentId == ValidPaymentOrder.Id &&
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
        ArrangeExisitingPaymentOrder();

        // Act
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None));

        // Assert — nenhuma entrada de ledger pode ser gravada se o saldo falhou
        _ledgerRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<LedgerEntry>>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    // TC4 · Rethrow de exceção genérica
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenUnexpectedExceptionOccurs_ShouldRethrowOriginalException()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeExisitingPaymentOrder();

        var unexpectedException = new InvalidOperationException("DB fora do ar");
        _balanceService
            .Setup(x => x.GetBalanceAsync(payerId))
            .ThrowsAsync(unexpectedException);

        // Act
        var act = () => _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

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
        ArrangeExisitingPaymentOrder();

        // SaveAsync falha quando tentamos salvar a transação como Failed
        _transactionRepository
            .Setup(x => x.SaveAsync(It.Is<Transaction>(t => t.Status == TransactionStatus.FAILED)))
            .ThrowsAsync(new Exception("Erro ao persistir Failed"));

        // Act
        var act = () => _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

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
        ArrangeExisitingPaymentOrder();

        // Act
        await _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None);

        // Assert
        _lockService.Verify(x => x.AcquireLockAsync(payerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDisposeLock_EvenWhenExceptionIsThrown()
    {
        // Arrange
        ArrangeNoExistingTransaction();
        ArrangeInsufficientBalance(50m);
        ArrangeExisitingPaymentOrder();

        // Act
        await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            _sut.ExecuteAsync(ValidPaymentOrder.Id, payerId, receiverId, amount, idempotencyKey, CancellationToken.None));

        // Assert — using garante o Dispose independente de exceção
        _lockHandle.Verify(x => x.Dispose(), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers privados
    // ─────────────────────────────────────────────────────────────

    private void ArrangeNoExistingTransaction()
        => _transactionRepository
            .Setup(x => x.GetByIdempotencyKeyAsync(idempotencyKey))
            .ReturnsAsync((Transaction?)null);

    private void ArrangeSufficientBalance(decimal balanceValue)
        => _balanceService
            .Setup(x => x.GetBalanceAsync(payerId))
            .ReturnsAsync(new Money(balanceValue, CurrencyType.BRL));

    private void ArrangeInsufficientBalance(decimal balanceValue)
        => _balanceService
            .Setup(x => x.GetBalanceAsync(payerId))
            .ReturnsAsync(new Money(balanceValue, CurrencyType.BRL));

    private void ArrangeExisitingPaymentOrder()
    {
        ValidPaymentOrder.MarkAsWaitingConfirmation();
        ValidPaymentOrder.MarkAsReadyToTransfer();
        _paymentOrderRepository.Setup(r => r.GetByIdAsync(ValidPaymentOrder.Id))
                   .ReturnsAsync(ValidPaymentOrder);
    }
}