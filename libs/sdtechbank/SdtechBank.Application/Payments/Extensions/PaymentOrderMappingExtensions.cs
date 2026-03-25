using SdtechBank.Domain.Entities;
using SdtechBank.Domain.ValueObjects;
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
        var payerId = Guid.Parse(request.PayerId);
        var destination = request.Receiver.ToEntity();
        var amount = new Money(request.Amount, Domain.Enums.CurrencyEnum.BRL);

        return PaymentOrder.Create(payerId: payerId, destination: destination, amount: amount);
    }
}
