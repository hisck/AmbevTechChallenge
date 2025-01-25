using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using FluentValidation;

public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.SaleDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.ProductName).NotEmpty();
            item.RuleFor(x => x.UnitPrice).GreaterThan(0);
            item.RuleFor(x => x.Quantity).InclusiveBetween(1, 20)
                .WithMessage("Quantity must be between 1 and 20 items");
        });
    }
}