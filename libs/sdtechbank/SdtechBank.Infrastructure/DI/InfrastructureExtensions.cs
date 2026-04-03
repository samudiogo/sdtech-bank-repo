using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SdtechBank.Application.Common.Contracts;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.UseCases.CompletePayment;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.Application.Payments.UseCases.FailPayment;
using SdtechBank.Application.Transactions.Contracts.Events;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.Shared.Messaging;
using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Domain.Transactions.Entities;
using SdtechBank.Infrastructure.Accounts.Services;
using SdtechBank.Infrastructure.Ledger.Persistence;
using SdtechBank.Infrastructure.Messaging;
using SdtechBank.Infrastructure.Messaging.Persistence;
using SdtechBank.Infrastructure.PaymentsOrders.Persistence;
using SdtechBank.Infrastructure.Shared.Concurrency;
using SdtechBank.Infrastructure.Shared.Mongo;
using SdtechBank.Infrastructure.Transactions.Persistence;

namespace SdtechBank.Infrastructure.DI;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureCore(this IServiceCollection services, IConfiguration configuration)
    {
        AddMongoDbConfig(services, configuration);
        AddRabbitMqConfig(services, configuration);
        AddRepositoriesConfig(services);
        AddServicesConfig(services);
        AddEventsConfig(services);
        return services;
    }

    public static IServiceCollection AddWebApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddInfrastructureCore(services, configuration);
        AddValidators(services);
        return services;
    }

    public static IServiceCollection AddWorkerInfrastructureCore(this IServiceCollection services, IConfiguration configuration)
    {
        AddInfrastructureCore(services, configuration);
        AddIntegrationEventsConfig(services);
        services.AddHostedService<RabbitMqConsumer>();
        services.AddHostedService<OutboxPublisher>();
        return services;
    }


    private static void AddMongoDbConfig(IServiceCollection services, IConfiguration configuration)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        MongoDbClassMap.Register();

        var settings = configuration.GetSection("MongoDb").Get<MongoDbSettings>()!;
        services.Configure<MongoDbSettings>(opts => configuration.GetSection("MongoDb").Bind(opts));
        services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoDbContext(client, settings.DatabaseName);
        });

        // Inicialização de índices no startup
        services.AddSingleton<MongoDbIndexInitializer>();
    }

    private static void AddIntegrationEventsConfig(IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventTypeRegistry>(sp =>
        {
            var registry = new IntegrationEventTypeRegistry();

            registry.Register<PaymentCreatedIntegrationEvent>("payment.created");
            registry.Register<TransactionCompletedIntegrationEvent>("transaction.completed");
            registry.Register<PaymentNeedsAccountDataIntegrationEvent>("payment.waiting_for_dict");
            registry.Register<PaymentValidatedEventIntegrationEvent>("payment.validated");
            registry.Register<TransactionFailedIntegrationEvent>("transaction.failed");

            return registry;
        });
    }

    private static void AddEventsConfig(IServiceCollection services)
    {
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        //services.AddSingleton<IIntegrationEventTypeResolver, IntegrationEventTypeResolver>();

        services.Scan(scan => scan
                .FromAssemblies(typeof(IEventHandler<>).Assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
    }

    private static void AddRabbitMqConfig(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(opts => configuration.GetSection("RabbitMq").Bind(opts));
        services.Configure<RabbitMqQueueSettings>(opts => configuration.GetSection("RabbitMqQueues").Bind(opts));

        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        services.AddTransient<IEventPublisher, RabbitMqEventPublisher>();
    }

    private static void AddRepositoriesConfig(IServiceCollection services)
    {
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();

        services.AddScoped<IInboxRepository, InboxRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

    }

    private static void AddServicesConfig(IServiceCollection services)
    {
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();
        services.AddScoped<IAccountLockService, InMemoryAccountLockService>();
        services.AddScoped<IOutboxService, OutboxService>();
    }

    private static void AddValidators(IServiceCollection services)
    {
        services.AddScoped<CreatePaymentValidator>();
    }
}