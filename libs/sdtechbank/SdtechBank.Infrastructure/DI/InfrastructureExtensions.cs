using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using SdtechBank.Application.Contracts;
using SdtechBank.Application.Payments.CreatePayment;
using SdtechBank.Infrastructure.Data;
using SdtechBank.Infrastructure.Messaging;
using SdtechBank.Infrastructure.MongoDB;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Infrastructure.DI;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfraestructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddMongoDbConfig(services, configuration);
        AddRabbitMqConfig(services, configuration);
        AddUseCasesConfig(services);
        AddRepositoriesConfig(services);
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
    }

    private static void AddRabbitMqConfig(IServiceCollection services, IConfiguration configuration)
    {        
        services.Configure<RabbitMqSettings>(opts => configuration.GetSection("RabbitMq").Bind(opts));
        services.Configure<RabbitMqQueueSettings>(opts => configuration.GetSection("RabbitMqQueues").Bind(opts));

        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddScoped<IMessageConsumer, RabbitMqConsumer>();
    }

    private static void AddUseCasesConfig(IServiceCollection services)
    {
         services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
        
    }

    private static void AddRepositoriesConfig(IServiceCollection services)
    {
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
    }

    private static void AddValidators(IServiceCollection services)
    {
            services.AddScoped<CreatePaymentValidator>();
    }
}
