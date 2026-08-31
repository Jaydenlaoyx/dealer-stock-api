using Dapper;

namespace DealerStockApi.Data;

public class DatabaseInitializer
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    public DatabaseInitializer(
        DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection =
            _connectionFactory.CreateConnection();

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

        await connection.ExecuteAsync(
            createDealersTableSql);

        await connection.ExecuteAsync(
            createCarsTableSql);

        const string seedDealersSql = """
        INSERT INTO Dealers (Name, Username, PasswordHash)
        SELECT @Name, @Username, @PasswordHash
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM Dealers
            WHERE Username = @Username
        );
        """;

        await connection.ExecuteAsync(
            seedDealersSql,
            new
            {
                Name = "Melbourne Motors",
                Username = "dealer1",
                PasswordHash = "password123"
            });

        await connection.ExecuteAsync(
            seedDealersSql,
            new
            {
                Name = "City Cars",
                Username = "dealer2",
                PasswordHash = "password123"
        });

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
}