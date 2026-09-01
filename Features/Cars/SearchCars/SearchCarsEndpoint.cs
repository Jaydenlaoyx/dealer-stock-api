using Dapper;
using DealerStockApi.Data;
using FastEndpoints;
using System.Security.Claims;

namespace DealerStockApi.Features.Cars.SearchCars;

public class SearchCarsEndpoint
    : Endpoint<SearchCarsRequest, IEnumerable<SearchCarsResponse>>
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    public SearchCarsEndpoint(
        DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public override void Configure()
    {
        Get("/api/cars/search");
    }

    public override async Task HandleAsync(
        SearchCarsRequest req,
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
              AND (
                    @Make IS NULL
                    OR LOWER(Make) LIKE LOWER(@Make)
                  )
              AND (
                    @Model IS NULL
                    OR LOWER(Model) LIKE LOWER(@Model)
                  )
            ORDER BY Make, Model, Year;
            """;

        var make = string.IsNullOrWhiteSpace(req.Make)
            ? null
            : $"%{req.Make.Trim()}%";

        var model = string.IsNullOrWhiteSpace(req.Model)
            ? null
            : $"%{req.Model.Trim()}%";

        var cars =
            await connection.QueryAsync<SearchCarsResponse>(
                sql,
                new
                {
                    DealerId = dealerId,
                    Make = make,
                    Model = model
                });

        await Send.OkAsync(cars, ct);
    }
}