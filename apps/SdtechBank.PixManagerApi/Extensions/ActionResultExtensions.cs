using Microsoft.AspNetCore.Mvc;
using SdtechBank.Application.Common;
using SdtechBank.Application.Common.Errors;

namespace SdtechBank.PixManagerApi.Extensions;

public static class ActionResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        if(result.IsSuccess)
            return new OkObjectResult(result.Value);

        var firstError = result.Errors.First();

        return firstError.Type switch
        {
            ErrorType.Validation => new BadRequestObjectResult(result.Errors),
            ErrorType.Business => new UnprocessableEntityObjectResult(result.Errors),
            ErrorType.NotFound => new NotFoundObjectResult(result.Errors),
            ErrorType.Conflict => new ConflictObjectResult(result.Errors),
            _ => new ObjectResult(result.Errors) { StatusCode = 500 }
        };
    }
}
