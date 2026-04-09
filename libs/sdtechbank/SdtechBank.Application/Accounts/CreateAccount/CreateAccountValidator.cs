
using FluentValidation;

namespace SdtechBank.Application.Accounts.CreateAccount;

public class CreateAccountValidator: AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.FullName)
        .NotEmpty().WithMessage("SourceCode é obrigatório");
        
        RuleFor(x => x.Cpf)
           .NotEmpty().WithMessage("CPF é obrigatório")
           .Length(11).WithMessage("CPF deve ter 11 caracteres");

        RuleFor(x => x.BankCode)
        .NotEmpty().WithMessage("BankCode é obrigatório");
        
        RuleFor(x => x.Branch)
        .NotEmpty().WithMessage("Branch é obrigatório");
        
        RuleFor(x => x.AccountCode)
        .NotEmpty().WithMessage("AccountCode é obrigatório");

        RuleFor(x => x.AccountType)
        .NotEmpty().WithMessage("AccountType é obrigatório");
        
    }
}
