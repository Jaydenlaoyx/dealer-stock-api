using Dapper;
using DealerStockApi.Models;
using Microsoft.AspNetCore.Identity;

namespace DealerStockApi.Data;

public class DatabaseInitializer
{
    private readonly DatabaseConnectionFactory _connectionFactory;
    private readonly PasswordHasher<Dealer> _passwordHasher;

    public DatabaseInitializer(
        DatabaseConnectionFactory connectionFactory,
        PasswordHasher<Dealer> passwordHasher)
    {
        _connectionFactory = connectionFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string createDealersTableSql = """
            CREATE TABLE IF NOT EXISTS Dealers
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL
            );
            """;

        const string createCarsTableSql = """
            CREATE TABLE IF NOT EXISTS Cars
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DealerId INTEGER NOT NULL,
                Make TEXT NOT NULL,
                Model TEXT NOT NULL,
                Year INTEGER NOT NULL,
                StockLevel INTEGER NOT NULL CHECK (StockLevel >= 0),

                FOREIGN KEY (DealerId)
                    REFERENCES Dealers(Id)
                    ON DELETE CASCADE
            );
            """;

        await connection.ExecuteAsync(createDealersTableSql);
        await connection.ExecuteAsync(createCarsTableSql);

        await SeedDealerAsync(
            connection,
            "Melbourne Motors",
            "dealer1",
            "password123");

        await SeedDealerAsync(
            connection,
            "City Cars",
            "dealer2",
            "password123");

        const string seedCarsSql = """
            INSERT INTO Cars
                (DealerId, Make, Model, Year, StockLevel)
            SELECT
                @DealerId,
                @Make,
                @Model,
                @Year,
                @StockLevel
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM Cars
                WHERE DealerId = @DealerId
                  AND Make = @Make
                  AND Model = @Model
                  AND Year = @Year
            );
            """;

        await connection.ExecuteAsync(
            seedCarsSql,
            new
            {
                DealerId = 1,
                Make = "Audi",
                Model = "A4",
                Year = 2018,
                StockLevel = 5
            });

        await connection.ExecuteAsync(
            seedCarsSql,
            new
            {
                DealerId = 1,
                Make = "Toyota",
                Model = "Camry",
                Year = 2020,
                StockLevel = 3
            });

        await connection.ExecuteAsync(
            seedCarsSql,
            new
            {
                DealerId = 2,
                Make = "BMW",
                Model = "320i",
                Year = 2019,
                StockLevel = 4
            });
    }

    private async Task SeedDealerAsync(
        System.Data.IDbConnection connection,
        string name,
        string username,
        string password)
    {
        const string dealerExistsSql = """
            SELECT COUNT(1)
            FROM Dealers
            WHERE Username = @Username;
            """;

        var exists = await connection.ExecuteScalarAsync<bool>(
            dealerExistsSql,
            new
            {
                Username = username
            });

        if (exists)
        {
            return;
        }

        var dealer = new Dealer
        {
            Name = name,
            Username = username
        };

        dealer.PasswordHash =
            _passwordHasher.HashPassword(dealer, password);

        const string insertDealerSql = """
            INSERT INTO Dealers
                (Name, Username, PasswordHash)
            VALUES
                (@Name, @Username, @PasswordHash);
            """;

        await connection.ExecuteAsync(
            insertDealerSql,
            dealer);
    }
}