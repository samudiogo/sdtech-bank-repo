using SdtechBank.Application.Accounts.Contracts;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Enums;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Accounts.Services;

public class AccountBalanceService(ILedgerRepository ledgerRepository) : IAccountBalanceService
{
    public async Task<Money> GetBalanceAsync(Guid accountId)
    {
        var entries = await ledgerRepository.GetByAccountIdAsync(accountId);

        var credit = entries.Where(e => e.Type == LedgerEntryType.CREDIT)
                            .Sum(e => e.Amount.Value);
        var debit = entries.Where(e => e.Type == LedgerEntryType.DEBIT)
                            .Sum(e => e.Amount.Value);

        return new Money(credit - debit, CurrencyType.BRL);
    }
}
