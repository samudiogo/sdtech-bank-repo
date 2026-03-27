namespace SdtechBank.Application.Common.Errors
{
    public static class ErrorFactory
    {
        public static List<Error> FromValidation(this IEnumerable<FluentValidation.Results.ValidationFailure> failures)
            => failures.Select(e => new Error(
                e.ErrorCode ?? "VALIDATION_ERROR",
                e.ErrorMessage,
                ErrorType.Validation
            )).ToList();
    }
}

