using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SdtechBank.Application.Ports;
using SdtechBank.Infrastructure.Messaging;
using SdtechBank.Infrastructure.MongoDB;

namespace SdtechBank.Infrastructure.DI;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfraestructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddMongoDbConfig(services, configuration);
        AddRabbitMqConfig(services, configuration);
        AddUseCasesConfig(services);
        AddRepositoriesConfig(services);
        return services;
    }


    private static void AddMongoDbConfig(IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("MongoDb").Get<MongoDbSettings>()!;
        services.Configure<MongoDbSettings>(opts => configuration.GetSection("MongoDb").Bind(opts));
        services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoDbContext(client, settings.DatabaseName);
        });
    }

    private static void AddRabbitMqConfig(IServiceCollection services, IConfiguration configuration)
    {        
        services.Configure<RabbitMqSettings>(opts => configuration.GetSection("RabbitMq").Bind(opts));
        services.AddScoped<IMessageConsumer, RabbitMqConsumer>();
    }

    private static void AddUseCasesConfig(IServiceCollection services)
    {
        // TODO: adicionar as implementações dos casos de uso, por exemplo:
        // services.AddScoped<ITransferenciaUseCase, TransferenciaUseCase>();
    }

    private static void AddRepositoriesConfig(IServiceCollection services)
    {
        // TODO: adicionar as implementações dos repositórios, por exemplo:
        // services.AddScoped<IContaRepository, ContaRepository>();
    }
}
