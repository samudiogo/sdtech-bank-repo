
using SdtechBank.Domain.Ledger.Entities;

namespace SdtechBank.Domain.Ledger.Contracts;

public interface ILedgerRepository
{
    Task AddRangeAsync(IEnumerable<LedgerEntry> entries);
    Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId);
}
