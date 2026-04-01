using Scalar.AspNetCore;
using SdtechBank.Application.Common.DI;
using SdtechBank.Infrastructure.DI;
using SdtechBank.Infrastructure.Shared.Mongo;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks()
    .AddMongoDb(builder.Configuration["MongoDB:ConnectionString"]!, tags: new[] { "ready" })
    .AddRabbitMQ(sp => builder.Configuration["RabbitMQ:ConnectionString"]!, tags: new[] { "ready" });
;
builder.Services.AddWebApiInfrastructure(builder.Configuration);
builder.Services.AddWebApiApplication(builder.Configuration);



var app = builder.Build();

// Liveness (app subiu)
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => false // só verifica se app está rodando
});

// Readiness (infra OK)
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Desafio SDTECH Bank - Sistemas Distribuidos")
        .ForceDarkMode()
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithTheme(ScalarTheme.BluePlanet);
});

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<MongoDbIndexInitializer>();
    await initializer.InitializeAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
