using SdtechBank.Application.Payments.Resolvers.Specifications;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Application.Payments.Resolvers;

public sealed class InternalAccountReceiverStrategy(IAccountRepository repository, InternalBankSpecification specification) : IReceiverStrategy
{
    public string Name => nameof(InternalAccountReceiverStrategy);

    public bool CanResolve(PaymentOrder payment) => specification.IsSatisfiedBy(payment);

    public async Task<Guid?> ResolveAsync(PaymentOrder paymentOrder, CancellationToken cancellationToken)
    {
        var account = await repository.GetByBankAccountAsync(paymentOrder.Destination.BankAccount!, cancellationToken);

        return account?.Id;
    }
}
