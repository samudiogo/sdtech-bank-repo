using Microsoft.Extensions.Diagnostics.HealthChecks;
using SdtechBank.Infrastructure.Messaging;

namespace SdtechBank.PixManagerApi;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IRabbitMqConnection _connection;

    public RabbitMqHealthCheck(IRabbitMqConnection connection)
    {
        _connection = connection;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isConnected = await _connection.IsConnectedAsync();

            if (isConnected)
                return HealthCheckResult.Healthy();

            return HealthCheckResult.Unhealthy("RabbitMQ não conectado");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Erro ao conectar no RabbitMQ", ex);
        }
    }
}