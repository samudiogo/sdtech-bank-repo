using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SdtechBank.Application.Common.Contracts;
using SdtechBank.Application.Payments.UseCases.CompletePayment;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.Application.Payments.UseCases.FailPayment;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Domain.Transactions.Entities;
using SdtechBank.Infrastructure.Concurrency;
using SdtechBank.Infrastructure.Messaging;
using SdtechBank.Infrastructure.MongoDB;
using SdtechBank.Infrastructure.Persistence;

namespace SdtechBank.Infrastructure.DI;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfraestructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddMongoDbConfig(services, configuration);
        AddRabbitMqConfig(services, configuration);
        AddUseCasesConfig(services);
        AddRepositoriesConfig(services);
        AddServicesConfig(services);
        AddValidators(services);
        return services;
    }


    private static void AddMongoDbConfig(IServiceCollection services, IConfiguration configuration)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        var settings = configuration.GetSection("MongoDb").Get<MongoDbSettings>()!;
        services.Configure<MongoDbSettings>(opts => configuration.GetSection("MongoDb").Bind(opts));
        services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoDbContext(client, settings.DatabaseName);
        });

        BsonClassMap.RegisterClassMap<PaymentOrder>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(c => c.Id);
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<Transaction>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(c => c.Id);
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<LedgerEntry>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(c => c.Id);
            cm.SetIgnoreExtraElements(true);
        });
    }

    private static void AddRabbitMqConfig(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(opts => configuration.GetSection("RabbitMq").Bind(opts));
        services.Configure<RabbitMqQueueSettings>(opts => configuration.GetSection("RabbitMqQueues").Bind(opts));

        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddScoped<IMessageConsumer, RabbitMqConsumer>();
    }

    private static void AddUseCasesConfig(IServiceCollection services)
    {
        services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
        services.AddScoped<IProcessPaymentCreatedUseCase, ProcessPaymentCreatedUseCase>();
        services.AddScoped<ICompletePaymentUseCase, CompletePaymentUseCase>();
        services.AddScoped<IFailPaymentUseCase, FailPaymentUseCase>();

    }

    private static void AddRepositoriesConfig(IServiceCollection services)
    {
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();
    }

    private static void AddServicesConfig(IServiceCollection services)
    {
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();
        services.AddScoped<IAccountLockService, InMemoryAccountLockService>();
    }

    private static void AddValidators(IServiceCollection services)
    {
        services.AddScoped<CreatePaymentValidator>();
    }
}
