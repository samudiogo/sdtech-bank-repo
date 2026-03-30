using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Application.Payments.Contracts.Events;

public static class PaymentOrderEventMappingExtensions
{
    public static PaymentCreatedEvent ToPaymentCreatedEvent(this PaymentOrder paymentOrder)
    {
        return new PaymentCreatedEvent
        {
            PaymentId = paymentOrder.Id,
            Amount = paymentOrder.Amount.Value,
            Currency = paymentOrder.Amount.Currency.ToString(),
            PayerId = paymentOrder.PayerId,
            Destination = paymentOrder.Destination.ToPaymentDestinationEvent()
        };
    }

    public static PaymentDestinationEvent ToPaymentDestinationEvent(this PaymentDestination paymentDestination)
    {
        return new PaymentDestinationEvent
        {
            PixKey = paymentDestination.PixKey,
            BankAccount = paymentDestination.BankAccount?.ToBankAccountEvent()
        };
    }

    public static BankAccountEvent ToBankAccountEvent(this BankAccount bankAccount)
    {
        return new BankAccountEvent
        {
            FullName = bankAccount.FullName,
            Cpf = bankAccount.Cpf,
            BankCode = bankAccount.BankCode,
            Branch = bankAccount.Branch,
            Account = bankAccount.Account
        };
    }
}