using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Application.Payments.Resolvers.Specifications;

public sealed class InternalBankSpecification : IReceiverSpecification
{
    private const string SdtechBankCode = "999";

    public bool IsSatisfiedBy(PaymentOrder paymentOrder) => paymentOrder.Destination.HasBankAccount() && paymentOrder.Destination.BankAccount!.BankCode == SdtechBankCode;
}
