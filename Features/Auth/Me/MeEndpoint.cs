using FastEndpoints;
using System.Security.Claims;
using DealerStockApi.Extensions;

namespace DealerStockApi.Features.Auth.Me;

public class MeEndpoint : EndpointWithoutRequest<MeResponse>
{
    public override void Configure()
    {
        Get("/api/auth/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var username =
            User.FindFirstValue("Username");

        if (!User.TryGetDealerId(out var dealerId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(
            new MeResponse
            {
                DealerId = dealerId,
                Username = username ?? string.Empty
            },
            ct);
    }
}