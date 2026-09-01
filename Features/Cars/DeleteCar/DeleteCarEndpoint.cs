using Dapper;
using DealerStockApi.Data;
using FastEndpoints;
using System.Security.Claims;

namespace DealerStockApi.Features.Cars.DeleteCar;

public class DeleteCarEndpoint : EndpointWithoutRequest
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    public DeleteCarEndpoint(
        DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public override void Configure()
    {
        Delete("/api/cars/{id:int}");
    }

    public override async Task HandleAsync(CancellationToken ct)
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
            DELETE FROM Cars
            WHERE Id = @Id
              AND DealerId = @DealerId;
            """;

        var rowsAffected =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    Id = carId,
                    DealerId = dealerId
                });

        if (rowsAffected == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}