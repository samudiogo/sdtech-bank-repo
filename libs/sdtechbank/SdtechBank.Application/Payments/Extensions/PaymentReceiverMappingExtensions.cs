using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.Application.Payments.Extensions;

public static class PaymentReceiverMappingExtensions
{
    public static PaymentDestination ToEntity(this PaymentReceiverRequest receiver)
    {
        if (!string.IsNullOrWhiteSpace(receiver.PixKey))
            return PaymentDestination.FromPixKey(receiver.PixKey!);
                
        return PaymentDestination.FromBankAccount(receiver.BankAccount!.ToEntity());
    }
}
