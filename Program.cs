using DealerStockApi.Data;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseConnectionFactory>();
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services
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

app.UseFastEndpoints()
   .UseSwaggerGen();

app.Run();