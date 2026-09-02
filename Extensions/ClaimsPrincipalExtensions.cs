using System.Security.Claims;

namespace DealerStockApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetDealerId(
        this ClaimsPrincipal user,
        out int dealerId)
    {
        var dealerIdValue =
            user.FindFirstValue("DealerId");

        return int.TryParse(
            dealerIdValue,
            out dealerId);
    }
}