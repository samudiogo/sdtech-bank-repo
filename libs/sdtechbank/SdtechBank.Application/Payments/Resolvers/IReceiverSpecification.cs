using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Application.Payments.Resolvers;

public interface IReceiverSpecification
{
    bool IsSatisfiedBy(PaymentOrder paymentOrder);
}

public interface IReceiverStrategy
{
    string Name { get; }

    bool CanResolve(PaymentOrder payment);

    Task<Guid?> ResolveAsync(PaymentOrder paymentOrder, CancellationToken cancellationToken);
}