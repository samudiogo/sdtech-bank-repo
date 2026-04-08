using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Application.Payments.Resolvers;

public class ReceiverResolverChain(IEnumerable<IReceiverResolutionStep> steps) : IReceiverResolver
{
    public async Task<Guid?> ResolveAsync(PaymentOrder payment, CancellationToken cancellationToken)
    {
        foreach (var step in steps)
        {
            var result = await step.ResolveAsync(payment, cancellationToken);
            if (result is not null)
                return result;
        }
        return null;
    }
}
