using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using SdtechBank.Application.Accounts.Contracts;
using SdtechBank.Application.Accounts.CreateAccount;
using SdtechBank.Application.Accounts.Services;
using SdtechBank.Application.Deposits.UseCases;
using SdtechBank.Application.DictServices;
using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.Resolvers;
using SdtechBank.Application.Payments.Resolvers.Specifications;
using SdtechBank.Application.Payments.UseCases.CompletePayment;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.Application.Payments.UseCases.FailPayment;
using SdtechBank.Application.Payments.UseCases.ResolvePaymentDictUseCase;
using SdtechBank.Application.Payments.UseCases.ValidatePayment;
using SdtechBank.Application.Transactions.Contracts.Events;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.Domain.PaymentOrders.Services;

namespace SdtechBank.Application.Common.DI;

public static class ApplicationExtensions
{
    private static IServiceCollection AddDIApplicationCore(IServiceCollection services)
    {
        AddProcessingUseCases(services);
        AddEventsConfig(services);
        AddIntegrationEventsConfig(services);
        AddResolversConfig(services);
        AddServicesConfig(services);
        AddPixHandlerConfig(services);
        AddHttpClientsServicesConfig(services);
        return services;
    }
    public static IServiceCollection AddWebApiApplication(this IServiceCollection services)
    {
        AddDIApplicationCore(services);
        AddInboundUseCases(services);
        AddValidators(services);
        return services;
    }

    public static IServiceCollection AddWorkerApplication(this IServiceCollection services)
    {
        AddDIApplicationCore(services);
        return services;
    }
    private static void AddIntegrationEventsConfig(IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventTypeRegistry>(sp =>
        {
            var registry = new IntegrationEventTypeRegistry();

            registry.Register<PaymentCreatedIntegrationEvent>("payment.created");
            registry.Register<TransactionCompletedIntegrationEvent>("transaction.completed");
            registry.Register<PaymentNeedsAccountDataIntegrationEvent>("payment.waiting_for_dict");
            registry.Register<PaymentValidatedIntegrationEvent>("payment.validated");
            registry.Register<TransactionFailedIntegrationEvent>("transaction.failed");

            return registry;
        });
    }

    private static void AddEventsConfig(IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();


        services.Scan(scan => scan
                .FromAssemblies(typeof(IIntegrationEventHandler<>).Assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
    }

    private static void AddInboundUseCases(IServiceCollection services)
    {
        services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
        services.AddScoped<ICreateDepositUseCase, CreateDepositUseCase>();
        services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
    }

    private static void AddResolversConfig(IServiceCollection services)
    {        
        services.AddScoped<InternalBankSpecification>();
        services.AddScoped<ExternalBankSpecification>();

        services.AddScoped<IReceiverStrategy, InternalAccountReceiverStrategy>();
        services.AddScoped<IReceiverStrategy, ExternalAccountReceiverStrategy>();

        services.AddScoped<IReceiverResolver, ReceiverResolver>();
    }

    private static void AddProcessingUseCases(IServiceCollection services)
    {
        services.AddScoped<IProcessPaymentCreatedUseCase, ProcessPaymentCreatedUseCase>();
        services.AddScoped<IValidatePaymentUseCase, ValidatePaymentUseCase>();
        services.AddScoped<ICompletePaymentUseCase, CompletePaymentUseCase>();
        services.AddScoped<IFailPaymentUseCase, FailPaymentUseCase>();
        services.AddScoped<IResolvePaymentDictUseCase, ResolvePaymentDictUseCase>();

    }

    private static void AddValidators(IServiceCollection services)
    {
        services.AddScoped<CreatePaymentValidator>();
        services.AddScoped<CreateDepositRequestValidator>();
        services.AddScoped<CreateAccountValidator>();
    }
    private static void AddServicesConfig(IServiceCollection services)
    {
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();
        services.AddScoped<IOutboxService, OutboxService>();
    }

    private static void AddPixHandlerConfig(IServiceCollection services)
    {
        services.AddSingleton<IPixKeyHandler, PhonePixKeyHandler>();
        services.AddSingleton<IPixKeyHandler, RandomPixKeyHandler>();
        services.AddSingleton<IPixKeyHandler, CpfPixKeyHandler>();
        services.AddSingleton<IPixKeyHandler, CnpjPixKeyHandler>();
        services.AddSingleton<IPixKeyHandler, EmailPixKeyHandler>();
        services.AddSingleton<PixKeyResolver>();
    }

    private static void AddHttpClientsServicesConfig(IServiceCollection services)
    {
        services.AddHttpClient<IDictClient, DictClient>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var host = config["Services:DictApi:Host"];
            var port = config["Services:DictApi:Port"];

            client.BaseAddress = new Uri($"http://{host}:{port}");
        });
    }
}
