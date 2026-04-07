using SdtechBank.Application.Common.DI;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.Infrastructure.DI;
using SdtechBank.Infrastructure.Shared.Mongo;


var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddScoped<ProcessPaymentCreatedUseCase>();
builder.Services.AddWorkerInfrastructureCore(builder.Configuration);
builder.Services.AddWorkerApplication(builder.Configuration);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<MongoDbIndexInitializer>();
    await initializer.InitializeAsync();
}

await host.RunAsync();
