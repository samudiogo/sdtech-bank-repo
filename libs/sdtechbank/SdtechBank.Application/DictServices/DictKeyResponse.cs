using System.Text.Json.Serialization;

namespace SdtechBank.Application.DictServices;

public partial class DictKeyResponse
{
    [JsonPropertyName("key")]
    public string Key { get; init; } = default!;

    [JsonPropertyName("key_normalized")]
    public string KeyNormalized { get; init; } = default!;

    [JsonPropertyName("key_type")]
    public string KeyType { get; init; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; init; } = default!;

    [JsonPropertyName("owner")]
    public OwnerResponse Owner { get; init; } = default!;

    [JsonPropertyName("account")]
    public AccountResponse Account { get; init; } = default!;

    [JsonPropertyName("metadata")]
    public MetadataResponse Metadata { get; init; } = default!;
}

public partial class AccountResponse
{
    [JsonPropertyName("ispb")]    
    public string Ispb { get; init; } = default!;

    [JsonPropertyName("bank_code")]    
    public string BankCode { get; init; } = default!;

    [JsonPropertyName("bank_name")]
    public string BankName { get; init; } = default!;

    [JsonPropertyName("branch")]
    public string Branch { get; init; } = default!;

    [JsonPropertyName("number")]    
    public string Number { get; init; } = default!;

    [JsonPropertyName("digit")]    
    public string Digit { get; init; } = default!;

    [JsonPropertyName("account_type")]
    public string AccountType { get; init; } = default!;
}

public partial class MetadataResponse
{
    [JsonPropertyName("version")]
    public long Version { get; init; } = default!;
}

public partial class OwnerResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("document")]
    public string Document { get; init; } = default!;

    [JsonPropertyName("document_type")]
    public string DocumentType { get; init; } = default!;
}
