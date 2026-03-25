
using SdtechBank.Domain.Entities;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Shared.DTOs.Payments;
public static class PaymentMappingExtensions
{
    public static PaymentResponse ToResponse(this PaymentOrder entity)
    {
        return new PaymentResponse { Id = entity.Id, Status = entity.PaymentStatus.ToString() };
    }
}
