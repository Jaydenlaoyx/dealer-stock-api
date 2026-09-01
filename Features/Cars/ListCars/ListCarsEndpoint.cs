using Dapper;
using DealerStockApi.Data;
using FastEndpoints;
using System.Security.Claims;

namespace DealerStockApi.Features.Cars.ListCars;

public class ListCarsEndpoint
    : EndpointWithoutRequest<IEnumerable<ListCarsResponse>>
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    public ListCarsEndpoint(
        DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public override void Configure()
    {
        Get("/api/cars");
    }

    public override async Task HandleAsync(
        CancellationToken ct)
    {
        var dealerIdValue =
            User.FindFirstValue("DealerId");

        if (!int.TryParse(dealerIdValue, out var dealerId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        using var connection =
            _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                Id,
                Make,
                Model,
                Year,
                StockLevel
            FROM Cars
            WHERE DealerId = @DealerId
            ORDER BY Make, Model, Year;
            """;

        var cars =
            await connection.QueryAsync<ListCarsResponse>(
                sql,
                new
                {
                    DealerId = dealerId
                });

        await Send.OkAsync(cars, ct);
    }
}