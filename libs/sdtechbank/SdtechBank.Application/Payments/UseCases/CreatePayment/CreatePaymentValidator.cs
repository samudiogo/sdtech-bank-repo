using FluentValidation;
using SdtechBank.Application.Common;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.Application.Payments.UseCases.CreatePayment;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount deve ser maior que zero");

        RuleFor(x => x.PayerId)
            .NotEmpty().WithMessage("PayerId é obrigatório");
        
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("IdempotencyKey é obrigatório");

        RuleFor(x => x.Receiver)
            .NotNull().WithMessage("Receiver é obrigatório")
            .SetValidator(new PaymentReceiverRequestValidator());
    }
}

public class PaymentReceiverRequestValidator : AbstractValidator<PaymentReceiverRequest?>
{
    public PaymentReceiverRequestValidator()
    {
        RuleFor(x => x)
            .RequireExactlyOne(
                x => x?.PixKey,
                x => x?.BankAccount
            )
            .WithMessage("Informe PixKey ou BankAccount, mas não ambos");

        When(x => x?.BankAccount is not null, () =>
        {
            RuleFor(x => x!.BankAccount!).SetValidator(new BankAccountRequestValidator());
        });

    }
}

public class BankAccountRequestValidator : AbstractValidator<BankAccountRequest>
{
    public BankAccountRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("FullName é obrigatório");
        RuleFor(x => x.BankCode).NotEmpty().WithMessage("BankCode é obrigatório");
        RuleFor(x => x.Branch).NotEmpty().WithMessage("Branch é obrigatório");
        RuleFor(x => x.Account).NotEmpty().WithMessage("Account é obrigatório");
        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Length(11).WithMessage("CPF deve ter 11 caracteres");
    }
}
