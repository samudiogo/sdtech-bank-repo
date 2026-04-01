

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SdtechBank.Application.Payments.UseCases.CompletePayment;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.Application.Payments.UseCases.FailPayment;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;

namespace SdtechBank.Application.Common.DI;

public static class ApplicationExtensions
{
    public static IServiceCollection AddWebApiApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddInboundUseCases(services);
        return services;
    }

    public static IServiceCollection AddWorkerApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddProcessingUseCases(services);
        return services;
    }

    private static void AddInboundUseCases(IServiceCollection services)
    {
        services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
    }

    private static void AddProcessingUseCases(IServiceCollection services)
    {
        services.AddScoped<IProcessPaymentCreatedUseCase, ProcessPaymentCreatedUseCase>();
        services.AddScoped<ICompletePaymentUseCase, CompletePaymentUseCase>();
        services.AddScoped<IFailPaymentUseCase, FailPaymentUseCase>();

    }
}
