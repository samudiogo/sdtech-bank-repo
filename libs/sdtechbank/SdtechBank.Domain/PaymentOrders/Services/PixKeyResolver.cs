namespace SdtechBank.Domain.PaymentOrders.Services;
public sealed class PixKeyResolver
{
    private PixKeyResolver() { }
    private static readonly IPixKeyHandler[] handlers =
    [
        new EmailPixKeyHandler(),
        new RandomPixKeyHandler(),
        new CpfPixKeyHandler(),
        new CnpjPixKeyHandler(),
        new PhonePixKeyHandler()
    ];

    public static IPixKeyHandler Resolve(string value) => handlers.FirstOrDefault(x => x.CanResolve(value)) ??
                                    throw new InvalidOperationException("Tipo de chave Pix inválido.");
}
