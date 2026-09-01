using DealerStockApi.Data;
using DealerStockApi.Models;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseConnectionFactory>();
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<PasswordHasher<Dealer>>();

var jwtSigningKey =
    builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured.");

builder.Services
    .AddAuthenticationJwtBearer(options =>
    {
        options.SigningKey = jwtSigningKey;
    })
    .AddAuthorization()
    .AddFastEndpoints()
    .SwaggerDocument(options =>
    {
        options.DocumentSettings = settings =>
        {
            settings.Title = "Dealer Stock API";
            settings.Version = "v1";
        };
    });

var app = builder.Build();

var databaseInitializer =
    app.Services.GetRequiredService<DatabaseInitializer>();

await databaseInitializer.InitializeAsync();

app.UseAuthentication()
   .UseAuthorization()
   .UseFastEndpoints()
   .UseSwaggerGen();

app.Run();