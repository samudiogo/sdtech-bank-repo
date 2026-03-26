using Scalar.AspNetCore;
using SdtechBank.Infrastructure.DI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfraestructure(builder.Configuration);


var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Desafio SDTECH Bank - Sistemas Distribuidos")
        .ForceDarkMode()
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithTheme(ScalarTheme.BluePlanet);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
