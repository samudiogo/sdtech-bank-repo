using SdtechBank.Application.Common.Errors;

namespace SdtechBank.Application.Common;

public class Result<T> : Result
{
    public T? Value { get; }    

    private Result(bool isSuccess, T? value, IList<Error>? errors = null) : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return new(true, value);
    }

    public static new Result<T> Failure(IList<Error> errors)
        => new(false, default, errors);
}
