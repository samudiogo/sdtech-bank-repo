using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Shared.DTOs.Payments.Requests;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Application.Payments.Extensions;

public static class PaymentOrderMappingExtensions
{
    public static PaymentResponse ToResponse(this PaymentOrder entity)
    {
        return new PaymentResponse { Id = entity.Id, Status = entity.PaymentStatus.ToString() };
    }

    public static PaymentOrder ToEntity(this CreatePaymentRequest request)
    {
        var payerId = Guid.Parse(request.PayerId!);
        var destination = request.Receiver!.ToEntity();
        var amount = new Money(request.Amount!.Value, CurrencyType.BRL);

        return PaymentOrder.Create(payerId: payerId, destination: destination, amount: amount);
    }
}
