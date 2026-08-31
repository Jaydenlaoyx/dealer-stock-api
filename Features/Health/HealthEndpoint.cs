using FastEndpoints;

namespace DealerStockApi.Features.Health;

public class HealthEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/health");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new
        {
            status = "Healthy"
        }, ct);
    }
}