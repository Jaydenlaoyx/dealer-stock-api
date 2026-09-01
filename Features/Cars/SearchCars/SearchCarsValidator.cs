using FastEndpoints;
using FluentValidation;

namespace DealerStockApi.Features.Cars.SearchCars;

public class SearchCarsValidator : Validator<SearchCarsRequest>
{
    public SearchCarsValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Make) ||
                !string.IsNullOrWhiteSpace(x.Model))
            .WithMessage("At least one of make or model must be provided.");

        RuleFor(x => x.Make)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Make));

        RuleFor(x => x.Model)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Model));
    }
}