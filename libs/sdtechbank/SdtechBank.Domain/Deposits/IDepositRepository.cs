namespace SdtechBank.Domain.Deposits;

public interface IDepositRepository
{
    Task SaveAsync(Deposit deposit);
}
