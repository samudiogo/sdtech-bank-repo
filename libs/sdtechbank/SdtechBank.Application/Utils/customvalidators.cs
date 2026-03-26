using FluentValidation;

namespace SdtechBank.Application.Utils;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, T> RequireExactlyOne<T>(
        this IRuleBuilder<T, T> ruleBuilder,
        params Func<T, object?>[] properties)
    {
        return ruleBuilder.Must(obj =>
        {
            var filledCount = properties.Count(prop =>
            {
                var value = prop(obj);

                if (value is null)
                    return false;

                if (value is string str)
                    return !string.IsNullOrWhiteSpace(str);

                return true;
            });

            return filledCount == 1;
        })
        .WithMessage("Exatamente um dos campos deve ser informado");
    }
}
