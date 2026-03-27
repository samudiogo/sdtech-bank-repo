using SdtechBank.Application.Common;
using SdtechBank.Shared.DTOs.Payments.Requests;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Application.Payments.CreatePayment;

public interface ICreatePaymentUseCase
{
    Task<Result<PaymentResponse>> ExecuteAsync(CreatePaymentRequest request);
}
