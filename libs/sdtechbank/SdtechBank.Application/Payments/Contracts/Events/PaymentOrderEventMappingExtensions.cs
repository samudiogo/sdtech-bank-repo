using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Application.Payments.Contracts.Events;

public static class PaymentOrderEventMappingExtensions
{
    public static PaymentCreatedIntegrationEvent ToPaymentCreatedIntegrationEvent(this PaymentOrder paymentOrder)
    {
        return new PaymentCreatedIntegrationEvent
        {
            PaymentId = paymentOrder.Id,
            Amount = paymentOrder.Amount.Value,
            Currency = paymentOrder.Amount.Currency.ToString(),
            PayerId = paymentOrder.PayerId,
            Destination = paymentOrder.Destination.ToPaymentDestinationSnapshot()
        };
    }

    public static PaymentValidatedIntegrationEvent ToPaymentValidatedIntegrationEvent(this PaymentOrder paymentOrder, string correlationId, Guid receiverId) => new()
    {
        PaymentId = paymentOrder.Id,
        PayerId = paymentOrder.PayerId,
        Amount = paymentOrder.Amount,
        CorrelationId = correlationId,
        IdempotencyKey = paymentOrder.IdempotencyKey.Value,
        ReceiverId = receiverId,
        Destination = paymentOrder.Destination.ToPaymentDestinationSnapshot()
    };

    public static PaymentNeedsAccountDataIntegrationEvent ToPaymentNeedsAccountDataIntegrationEvent(this PaymentOrder paymentOrder) =>
        new() { PaymentId = paymentOrder.Id, PixKey = paymentOrder.Destination.PixKey!.Value };

    public static PaymentDestinationSnapshot ToPaymentDestinationSnapshot(this PaymentDestination paymentDestination)
    {
        return new PaymentDestinationSnapshot
        {
            PixKey = paymentDestination.PixKey?.Value,
            BankAccount = paymentDestination.BankAccount?.ToBankAccountSnapshot()
        };
    }

    public static BankAccountSnapshot ToBankAccountSnapshot(this BankAccount bankAccount)
    {
        return new BankAccountSnapshot
        {
            FullName = bankAccount.FullName,
            Cpf = bankAccount.Document,
            BankCode = bankAccount.BankCode,
            Branch = bankAccount.Branch,
            Account = bankAccount.Account
        };
    }
}