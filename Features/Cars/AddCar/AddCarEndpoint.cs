using Dapper;
using DealerStockApi.Data;
using FastEndpoints;
using System.Security.Claims;

namespace DealerStockApi.Features.Cars.AddCar;

public class AddCarEndpoint : Endpoint<AddCarRequest, AddCarResponse>
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    public AddCarEndpoint(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public override void Configure()
    {
        Post("/api/cars");
    }

    public override async Task HandleAsync(
        AddCarRequest req,
        CancellationToken ct)
    {
        var dealerIdValue = User.FindFirstValue("DealerId");

        if (!int.TryParse(dealerIdValue, out var dealerId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO Cars
                (DealerId, Make, Model, Year, StockLevel)
            VALUES
                (@DealerId, @Make, @Model, @Year, @StockLevel);

            SELECT last_insert_rowid();
            """;

        var carId = await connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                DealerId = dealerId,
                req.Make,
                req.Model,
                req.Year,
                req.StockLevel
            });

        var response = new AddCarResponse
        {
            Id = (int)carId,
            Make = req.Make,
            Model = req.Model,
            Year = req.Year,
            StockLevel = req.StockLevel
        };

        await Send.CreatedAtAsync<GetCar.GetCarEndpoint>(
            new { id = response.Id },
            response,
            cancellation: ct);
    }
}