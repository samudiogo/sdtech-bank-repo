namespace SdtechBank.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public string Host { get; set; } = default!;
    public string Port { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = default!;
    public string DefaultQueue { get; set; } = default!;
    public string DlqExchange { get; set; } = default!;
    public string DlqQueue { get; set; } = default!;
    public string QueueType { get; set; } = default!;
    public long MessageTtlMs { get; set; } = default!;
}

