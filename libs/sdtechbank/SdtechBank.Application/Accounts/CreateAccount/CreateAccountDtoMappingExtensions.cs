using SdtechBank.Domain.Accounts;

namespace SdtechBank.Application.Accounts.CreateAccount;

public static class CreateAccountDtoMappingExtensions
{
    public static CreateAccountResponse ToResponse(this Account entity)
    {
        return new CreateAccountResponse(
            AccountId: entity.Id,
            FullName: entity.FullName,
            Cpf: entity.Document,
            BankCode: entity.BankCode,
            Branch: entity.Branch,
            AccountCode: entity.AccountCode,
            Type: entity.Type.ToString(),
            Status: entity.Status.ToString(),
            CreatedAt: entity.CreatedAt);
    }
    public static Account ToEntity(this CreateAccountRequest request)
    {
        return Account.Create(
            fullName: request.FullName,
            document: request.Cpf,
            bankCode: request.BankCode,
            branch: request.Branch,
            accountCode: request.AccountCode,
            type: Enum.Parse<AccountType>(request.AccountType));
    }   

}
