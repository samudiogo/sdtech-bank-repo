using SdtechBank.Application.Common;
using SdtechBank.Application.Common.Errors;
using SdtechBank.Domain.Accounts.Contracts;


namespace SdtechBank.Application.Accounts.CreateAccount;

public interface ICreateAccountUseCase
{
    Task<Result<CreateAccountResponse>> RegisterAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
}

public class CreateAccountUseCase(IAccountRepository accountRepository, CreateAccountValidator validator) : ICreateAccountUseCase
{
    public async Task<Result<CreateAccountResponse>> RegisterAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request);

        if (!validationResult.IsSuccess)
            return Result<CreateAccountResponse>.Failure(validationResult.Errors);

        var account = request.ToEntity();

        await accountRepository.SaveAsync(account, cancellationToken);

        return Result<CreateAccountResponse>.Success(account.ToResponse());
    }

    private Result ValidateRequest(CreateAccountRequest request)
    {
        var result = validator.Validate(request);

        if (!result.IsValid)
            return Result.Failure(result.Errors.FromValidation());

        return Result.Success();
    }
}
