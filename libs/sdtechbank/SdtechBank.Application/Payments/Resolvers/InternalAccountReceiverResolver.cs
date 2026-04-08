using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Application.Payments.Resolvers;

public class InternalAccountReceiverResolver(IAccountRepository accountRepository) : IReceiverResolutionStep
{
    public async Task<Guid?> ResolveAsync(PaymentOrder payment, CancellationToken cancellationToken)
    {
        if (!payment.Destination.HasBankAccount()) return null;

        var account = await accountRepository.GetByBankAccountAsync(payment.Destination.BankAccount!, cancellationToken);

        return account?.Id;
    }
}
