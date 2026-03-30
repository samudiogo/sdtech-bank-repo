using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.PixManagerWorker.Consumers;

namespace SdtechBank.PixManagerWorker.Workers;

public class PaymentWorker(IServiceProvider serviceProvider, ILogger<PaymentWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PaymentWorker iniciado");

        // 🔥 Simulação de consumo (depois entra RabbitMQ)
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();

            var consumer = scope.ServiceProvider
                .GetRequiredService<PaymentCreatedConsumer>();

            // Simulando evento chegando
            var fakeEvent = new PaymentCreatedEvent
            {
                PaymentId = Guid.NewGuid(),
                Amount = 100,
                Currency = "BRL",
                PayerId = Guid.NewGuid(),
                Destination = new PaymentDestinationEvent(),
            };

            await consumer.HandleAsync(fakeEvent);

            await Task.Delay(5000, stoppingToken);
        }
    }

}

