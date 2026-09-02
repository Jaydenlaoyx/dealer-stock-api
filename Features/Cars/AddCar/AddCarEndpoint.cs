using Dapper;
using DealerStockApi.Data;
using FastEndpoints;
using DealerStockApi.Extensions;

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
        if (!User.TryGetDealerId(out var dealerId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        using var connection = _connectionFactory.CreateConnection();

        const string duplicateSql = """
            SELECT COUNT(1)
            FROM Cars
            WHERE DealerId = @DealerId
            AND LOWER(Make) = LOWER(@Make)
            AND LOWER(Model) = LOWER(@Model)
            AND Year = @Year;
            """;

        var duplicateExists =
            await connection.ExecuteScalarAsync<bool>(
                duplicateSql,
                new
                {
                    DealerId = dealerId,
                    Make = req.Make.Trim(),
                    Model = req.Model.Trim(),
                    req.Year
                });

        if (duplicateExists)
        {
            await Send.StringAsync(
                "A car with the same make, model and year already exists.",
                statusCode: StatusCodes.Status409Conflict,
                cancellation: ct);

            return;
        }

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
                Make = req.Make.Trim(),
                Model = req.Model.Trim(),
                req.Year,
                req.StockLevel
            });

        var response = new AddCarResponse
        {
            Id = (int)carId,
            Make = req.Make.Trim(),
            Model = req.Model.Trim(),
            Year = req.Year,
            StockLevel = req.StockLevel
        };

        await Send.CreatedAtAsync<GetCar.GetCarEndpoint>(
            new { id = response.Id },
            response,
            cancellation: ct);
    }
}