using Microsoft.AspNetCore.Mvc;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.Application.Payments.UseCases.GetPayments;
using SdtechBank.PixManagerApi.Extensions;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.PixManagerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(ICreatePaymentUseCase createPaymentUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPayments([FromServices] IGetPaymentsUseCase usecase, CancellationToken cancellationToken)
    {
        var result  = await usecase.ExecuteAsync(cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentOrder(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await createPaymentUseCase.ExecuteAsync(request, cancellationToken);

        return result.ToActionResult();
    }
}
