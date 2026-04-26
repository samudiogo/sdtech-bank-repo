using SdtechBank.Application.Payments.Resolvers.Specifications;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Application.Payments.Resolvers;

public sealed class ExternalAccountReceiverStrategy(IAccountRepository repository, ExternalBankSpecification specification) : IReceiverStrategy
{
    private const string LedgerCode = "00001-1";
    public string Name => nameof(ExternalAccountReceiverStrategy);
    public bool CanResolve(PaymentOrder payment) => specification.IsSatisfiedBy(payment);

    public async Task<Guid?> ResolveAsync(PaymentOrder paymentOrder, CancellationToken cancellationToken)
    {
        var account = await repository.GetByAccountCodeAsync(LedgerCode, cancellationToken);

        return account?.Id;
    }
}