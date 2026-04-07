using FluentValidation;

namespace SdtechBank.Application.Deposits.UseCases;

public sealed class CreateDepositRequestValidator : AbstractValidator<CreateDepositRequest>
{
    public CreateDepositRequestValidator()
    {
        RuleFor(x => x.Amount)
        .GreaterThan(0).WithMessage("Amount deve ser maior que zero");

        RuleFor(x => x.CreditAccountId)
        .NotEmpty().WithMessage("CreditAccountId é obrigatório");

        RuleFor(x => x.SourceCode)
        .NotEmpty().WithMessage("SourceCode é obrigatório");

    }
}