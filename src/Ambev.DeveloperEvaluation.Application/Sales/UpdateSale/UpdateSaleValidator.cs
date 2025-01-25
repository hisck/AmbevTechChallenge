using FluentValidation;


namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale
{
    public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
    {
        public UpdateSaleValidator()
        {
            RuleFor(x => x.SaleNumber)
                .NotEmpty()
                .Matches(@"^SALE-\d{8}-[A-F0-9]{8}$")
                .WithMessage("Invalid sale number format");

            RuleFor(x => x.SaleDate).NotEmpty();
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.CustomerName).NotEmpty();
            RuleFor(x => x.BranchId).NotEmpty();
            RuleFor(x => x.BranchName).NotEmpty();
            RuleFor(x => x.Items).NotEmpty();

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId).NotEmpty();
                item.RuleFor(x => x.ProductName).NotEmpty();
                item.RuleFor(x => x.UnitPrice).GreaterThan(0);
                item.RuleFor(x => x.Quantity).InclusiveBetween(1, 20);
            });
        }
    }
}
