using SdtechBank.Application.Common;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Application.Payments.UseCases.GetPayments;

public interface IGetPaymentsUseCase
{
    Task<Result<IEnumerable<PaymentDtoResponse>>> ExecuteAsync(CancellationToken ct);
}

public sealed class GetPaymentsUseCase(IPaymentOrderRepository repository) : IGetPaymentsUseCase
{
    public async Task<Result<IEnumerable<PaymentDtoResponse>>> ExecuteAsync(CancellationToken ct)
    {
        IEnumerable<PaymentOrder> payments = await repository.GetAllAsync(ct);

        var responseData = payments.Select(p => new PaymentDtoResponse
        {
            Id = p.Id,
            Amount = p.Amount.Value,
            Status = p.PaymentStatus.ToString(),
            CreatedAt = p.CreatedAt
        });
        
        return Result<IEnumerable<PaymentDtoResponse>>.Success(responseData);
    }
}