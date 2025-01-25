using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale
{
    public class CancelSaleValidator : AbstractValidator<CancelSaleCommand>
    {
        public CancelSaleValidator()
        {
            RuleFor(x => x.SaleNumber)
                .NotEmpty()
                .Matches(@"^SALE-\d{8}-[A-F0-9]{8}$")
                .WithMessage("Invalid sale number format");
        }
    }
}
