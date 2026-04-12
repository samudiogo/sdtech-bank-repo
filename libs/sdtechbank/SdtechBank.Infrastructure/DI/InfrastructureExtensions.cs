using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SdtechBank.Application.Abstractions.Resilience;
using SdtechBank.Application.Accounts.Contracts;
using SdtechBank.Application.Common.Contracts;
using SdtechBank.Application.Messaging;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.Deposits;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Infrastructure.Accounts;
using SdtechBank.Infrastructure.Deposits;
using SdtechBank.Infrastructure.Ledger.Persistence;
using SdtechBank.Infrastructure.Messaging;
using SdtechBank.Infrastructure.Messaging.Persistence;
using SdtechBank.Infrastructure.PaymentsOrders.Persistence;
using SdtechBank.Infrastructure.Resilience;
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
        return services;
    }

    public static IServiceCollection AddWebApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddInfrastructureCore(services, configuration);
        return services;
    }

    public static IServiceCollection AddWorkerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddInfrastructureCore(services, configuration);
        services.AddHostedService<RabbitMqConsumer>();
        services.AddHostedService<OutboxPublisher>();
        AddResilienceConfig(services);
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

    private static void AddRabbitMqConfig(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(opts => configuration.GetSection("RabbitMq").Bind(opts));
        services.Configure<RabbitMqQueueSettings>(opts => configuration.GetSection("RabbitMqQueues").Bind(opts));

        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        services.AddTransient<IEventPublisher, RabbitMqEventPublisher>();

        services.AddScoped<IDlqPublisher, DlqPublisher>();

        services.Configure<OutboxPublisherSettings>(opts => configuration.GetSection("OutboxPublisher").Bind(opts));
    }

    private static void AddRepositoriesConfig(IServiceCollection services)
    {
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<IDepositRepository, DepositRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();

        services.AddScoped<IInboxRepository, InboxRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.AddScoped<IAccountRepository, AccountRepository>();

    }

    private static void AddServicesConfig(IServiceCollection services)
    {        
        services.AddScoped<IAccountLockService, InMemoryAccountLockService>();
    }

    private static void AddResilienceConfig(IServiceCollection services)
    {        
        services.AddScoped<IErrorClassifier, ErrorClassifier>();
        services.AddScoped<IRetryPolicy, RetryPolicy>();
    }
}