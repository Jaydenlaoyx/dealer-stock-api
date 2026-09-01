using Dapper;
using DealerStockApi.Data;
using DealerStockApi.Models;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;

namespace DealerStockApi.Features.Auth.Login;

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly DatabaseConnectionFactory _connectionFactory;
    private readonly PasswordHasher<Dealer> _passwordHasher;
    private readonly IConfiguration _configuration;

    public LoginEndpoint(
        DatabaseConnectionFactory connectionFactory,
        PasswordHasher<Dealer> passwordHasher,
        IConfiguration configuration)
    {
        _connectionFactory = connectionFactory;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        LoginRequest req,
        CancellationToken ct)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        const string sql = """
            SELECT Id,
                   Name,
                   Username,
                   PasswordHash
            FROM Dealers
            WHERE Username = @Username;
            """;

        var dealer =
            await connection.QuerySingleOrDefaultAsync<Dealer>(
                sql,
                new
                {
                    req.Username
                });

        if (dealer is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                dealer,
                dealer.PasswordHash,
                req.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var signingKey =
            _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException(
                "JWT signing key is not configured.");

        var token = JwtBearer.CreateToken(options =>
        {
            options.SigningKey = signingKey;
            options.ExpireAt = DateTime.UtcNow.AddHours(1);

            options.User.Claims.Add(
                ("DealerId", dealer.Id.ToString()));

            options.User.Claims.Add(
                ("Username", dealer.Username));
        });

        await Send.OkAsync(
            new LoginResponse
            {
                Token = token,
                DealerName = dealer.Name
            },
            ct);
    }
}