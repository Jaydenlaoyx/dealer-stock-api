using FastEndpoints;
using FluentValidation;

namespace DealerStockApi.Features.Cars.UpdateStock;

public class UpdateStockValidator : Validator<UpdateStockRequest>
{
    public UpdateStockValidator()
    {
        RuleFor(x => x.StockLevel)
            .GreaterThanOrEqualTo(0);
    }
}