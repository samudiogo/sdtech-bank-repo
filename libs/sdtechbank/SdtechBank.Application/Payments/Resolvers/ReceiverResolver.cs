using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Application.Payments.Resolvers;

public class ReceiverResolver(IEnumerable<IReceiverStrategy> strategies) : IReceiverResolver
{
    public async Task<Guid?> ResolveAsync(PaymentOrder payment, CancellationToken cancellationToken)
    {
        foreach (var strategy in strategies)
        {
            if (!strategy.CanResolve(payment))
                continue;            
            var result = await strategy.ResolveAsync(payment, cancellationToken);
            return result;
        }
        return null;
    }
}
