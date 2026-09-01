using Dapper;
using DealerStockApi.Data;
using FastEndpoints;
using System.Security.Claims;

namespace DealerStockApi.Features.Cars.GetCar;

public class GetCarEndpoint : EndpointWithoutRequest<GetCarResponse>
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    public GetCarEndpoint(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public override void Configure()
    {
        Get("/api/cars/{id:int}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var dealerIdValue = User.FindFirstValue("DealerId");

        if (!int.TryParse(dealerIdValue, out var dealerId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var carId = Route<int>("id");

        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                Id,
                Make,
                Model,
                Year,
                StockLevel
            FROM Cars
            WHERE Id = @Id
              AND DealerId = @DealerId;
            """;

        var car = await connection.QuerySingleOrDefaultAsync<GetCarResponse>(
            sql,
            new
            {
                Id = carId,
                DealerId = dealerId
            });

        if (car is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(car, ct);
    }
}