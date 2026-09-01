namespace DealerStockApi.Features.Auth.Login;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public string DealerName { get; set; } = string.Empty;
}