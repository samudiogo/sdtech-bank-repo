using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SdtechBank.Application.Common;
using SdtechBank.Application.Common.Errors;
using SdtechBank.PixManagerApi.Extensions;

namespace SdtechBank.PixManagerApi.Tests;

public class ActionResultExtensionsTests
{
    [Fact]
    public void ToActionResult_WhenSuccess_ShouldReturn200WithValue()
    {
        var result = Result<string>.Success("ok");

        var actionResult = result.ToActionResult();

        actionResult.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be("ok");

    }

    [Theory]
    [InlineData(ErrorType.Validation, typeof(BadRequestObjectResult))]
    [InlineData(ErrorType.Business, typeof(UnprocessableEntityObjectResult))]
    [InlineData(ErrorType.NotFound, typeof(NotFoundObjectResult))]
    [InlineData(ErrorType.Conflict, typeof(ConflictObjectResult))]
    public void ToActionResult_WhenKnownErrorType_ShouldReturnCorrectStatusCode(
        ErrorType errorType, Type expectedType)
    {
        var result = Result<string>.Failure([new Error(string.Empty, "msg", errorType)]);

        var actionResult = result.ToActionResult();

        actionResult.Should().BeOfType(expectedType);
    }

    [Fact]
    public void ToActionResult_WhenUnknownErrorType_ShouldReturn500()
    {
        var result = Result<string>.Failure([new Error(string.Empty, "unexpected", (ErrorType)999)]);

        var actionResult = result.ToActionResult() as ObjectResult;

        actionResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public void ToActionResult_WhenFailure_ShouldReturnErrorsInBody()
    {
        var error = new Error("0001", "campo obrigatório", ErrorType.Validation);
        var result = Result<string>.Failure([error]);

        var actionResult = result.ToActionResult() as BadRequestObjectResult;

        actionResult!.Value.Should().BeEquivalentTo(new[] { error });
    }

    [Fact]
    public void ToActionResult_WhenMultipleErrors_ShouldUseFirsErrorTypeToDecideStatus()
    {
        // O switch bate no PRIMEIRO erro — os demais apenas vão no body
        var errors = new[]
        {
            new Error("001", "campo A inválido",ErrorType.Validation),
            new Error("002",   "entidade não encontrada",ErrorType.NotFound)
        };
        var result = Result<string>.Failure(errors);

        var actionResult = result.ToActionResult();

        // Status ditado pelo primeiro (Validation → 400)
        actionResult.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().BeEquivalentTo(errors); // body tem todos
    }
}
