using SdtechBank.Domain.Transactions.Entities;

namespace SdtechBank.Domain.Transactions.Contracts;

public interface ITransactionRepository
{
    Task SaveAsync(Transaction transaction);

    Task<Transaction> GetByIdAsync(int id);
    Task<Transaction> GetByIdempontencyKeyAsync(string idempontencyKey);
}
