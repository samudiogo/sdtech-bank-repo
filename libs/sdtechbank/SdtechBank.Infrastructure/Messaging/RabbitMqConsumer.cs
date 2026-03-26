using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SdtechBank.Application.Contracts;

namespace SdtechBank.Infrastructure.Messaging;

public class RabbitMqConsumer : IMessageConsumer
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConsumer> _logger;
    public RabbitMqConsumer(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqConsumer> logger)
    {
        _settings = settings.Value;
        _logger = logger;

    }
    public Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
