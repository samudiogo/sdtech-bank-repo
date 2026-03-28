using Microsoft.AspNetCore.Mvc;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.PixManagerApi.Extensions;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.PixManagerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController (ICreatePaymentUseCase createPaymentUseCase) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult> CreatePaymentOrder(CreatePaymentRequest request)
    {
        var result = await createPaymentUseCase.ExecuteAsync(request);

        return result.ToActionResult();
    }
}
