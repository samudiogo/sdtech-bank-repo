using FluentAssertions;
using Moq;
using SdtechBank.Application.Accounts.Services;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Tests.Accounts;

public class AccountBalanceServiceTests
{
    private readonly Mock<ILedgerRepository> _ledgerRepository = new();
    private readonly AccountBalanceService _sut;

    public AccountBalanceServiceTests()
        => _sut = new AccountBalanceService(_ledgerRepository.Object);

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnCreditMinusDebit()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            LedgerEntry.CreateCredit(Guid.NewGuid(), accountId, new Money(500, CurrencyType.BRL)),
            LedgerEntry.CreateCredit(Guid.NewGuid(), accountId, new Money(200, CurrencyType.BRL)),
            LedgerEntry.CreateDebit(Guid.NewGuid(),  accountId, new Money(150, CurrencyType.BRL)),
        };

        _ledgerRepository.Setup(r => r.GetByAccountIdAsync(accountId)).ReturnsAsync(entries);

        // Act
        var balance = await _sut.GetBalanceAsync(accountId);

        // Assert
        balance.Value.Should().Be(550); // 700 - 150
        balance.Currency.Should().Be(CurrencyType.BRL);
    }

    [Fact]
    public async Task GetBalanceAsync_WhenNoEntries_ShouldReturnZero()
    {
        _ledgerRepository.Setup(r => r.GetByAccountIdAsync(It.IsAny<Guid>())).ReturnsAsync([]);

        var balance = await _sut.GetBalanceAsync(Guid.NewGuid());

        balance.Value.Should().Be(0);
    }

    [Fact]
    public async Task GetBalanceAsync_WhenOnlyDebits_ShouldReturnNegativeBalance()
    {
        var accountId = Guid.NewGuid();
        var entries = new List<LedgerEntry>
        {
            LedgerEntry.CreateDebit(Guid.NewGuid(), accountId, new Money(300, CurrencyType.BRL)),
        };

        _ledgerRepository.Setup(r => r.GetByAccountIdAsync(accountId)).ReturnsAsync(entries);

        var balance = await _sut.GetBalanceAsync(accountId);

        balance.Value.Should().Be(-300);
    }
}