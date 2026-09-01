using Dapper;
using DealerStockApi.Data;
using FastEndpoints;
using System.Security.Claims;

namespace DealerStockApi.Features.Cars.UpdateStock;

public class UpdateStockEndpoint
    : Endpoint<UpdateStockRequest, UpdateStockResponse>
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    public UpdateStockEndpoint(
        DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public override void Configure()
    {
        Put("/api/cars/{id:int}/stock");
    }

    public override async Task HandleAsync(
        UpdateStockRequest req,
        CancellationToken ct)
    {
        var dealerIdValue =
            User.FindFirstValue("DealerId");

        if (!int.TryParse(dealerIdValue, out var dealerId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var carId = Route<int>("id");

        using var connection =
            _connectionFactory.CreateConnection();

        const string sql = """
            UPDATE Cars
            SET StockLevel = @StockLevel
            WHERE Id = @Id
              AND DealerId = @DealerId;
            """;

        var rowsAffected =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    Id = carId,
                    DealerId = dealerId,
                    req.StockLevel
                });

        if (rowsAffected == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(
            new UpdateStockResponse
            {
                Id = carId,
                StockLevel = req.StockLevel
            },
            ct);
    }
}