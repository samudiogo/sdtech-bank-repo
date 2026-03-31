using SdtechBank.Application.Payments.Consumers;
using SdtechBank.Application.Transactions.Consumers;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<PaymentCreatedConsumer>();
builder.Services.AddScoped<ProcessPaymentCreatedUseCase>();
builder.Services.AddScoped<TransactionCompletedConsumer>();

var host = builder.Build();
host.Run();
