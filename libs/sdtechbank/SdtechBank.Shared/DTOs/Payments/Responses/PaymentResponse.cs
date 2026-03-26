namespace SdtechBank.Shared.DTOs.Payments.Responses;
public record PaymentResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = default!;

}
