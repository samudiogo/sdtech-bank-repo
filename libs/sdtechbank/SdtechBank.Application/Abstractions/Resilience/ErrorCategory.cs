namespace SdtechBank.Application.Abstractions.Resilience;

public enum ErrorCategory
{
    Business,
    Transient,
    Fatal
}