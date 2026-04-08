using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SdtechBank.Application.Deposits.UseCases;
using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.Resolvers;
using SdtechBank.Application.Payments.UseCases.CompletePayment;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.Application.Payments.UseCases.FailPayment;
using SdtechBank.Application.Payments.UseCases.ValidatePayment;
using SdtechBank.Application.Transactions.Contracts.Events;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;

namespace SdtechBank.Application.Common.DI;

public static class ApplicationExtensions
{
    public static IServiceCollection AddWebApiApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddInboundUseCases(services);
        AddEventsConfig(services);
        AddIntegrationEventsConfig(services);
        AddProcessingUseCases(services);
        AddValidators(services);
        return services;
    }

    public static IServiceCollection AddWorkerApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddProcessingUseCases(services);
        AddEventsConfig(services);
        AddIntegrationEventsConfig(services);
        AddResolversConfig(services);
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
    }

    private static void AddResolversConfig(IServiceCollection services)
    {
        services.AddScoped<IReceiverResolutionStep, InternalAccountReceiverResolver>();
        services.AddScoped<IReceiverResolver, ReceiverResolverChain>();        
    }

    private static void AddProcessingUseCases(IServiceCollection services)
    {
        services.AddScoped<IProcessPaymentCreatedUseCase, ProcessPaymentCreatedUseCase>();
        services.AddScoped<IValidatePaymentUseCase, ValidatePaymentUseCase>();
        services.AddScoped<ICompletePaymentUseCase, CompletePaymentUseCase>();
        services.AddScoped<IFailPaymentUseCase, FailPaymentUseCase>();
    }

    private static void AddValidators(IServiceCollection services)
    {
        services.AddScoped<CreatePaymentValidator>();
        services.AddScoped<CreateDepositRequestValidator>();
    }
}
