namespace DealerStockApi.Features.Cars.GetCar;

public class GetCarResponse
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int StockLevel { get; set; }
}