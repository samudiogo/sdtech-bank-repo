namespace SdtechBank.Shared.DTOs.Payments.Responses;
public record PaymentResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = default!;

}

public record PaymentDtoResponse
{
    public Guid Id { get; init; }
     public decimal Amount { get; init; }
    public string Status { get; init; } = default!;
    public DateTime CreatedAt { get; init; }

}