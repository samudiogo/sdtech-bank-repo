using SdtechBank.Application.Common.Errors;

namespace SdtechBank.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public IList<Error> Errors { get; }

    protected Result(bool isSuccess, IList<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? [];
    }

    public static Result Success()
        => new(true);

    public static Result Failure(IList<Error> errors)
        => new(false, errors);
}
