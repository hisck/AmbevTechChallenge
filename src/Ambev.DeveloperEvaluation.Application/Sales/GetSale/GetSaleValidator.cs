using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale
{
    public class GetSaleValidator : AbstractValidator<GetSaleCommand>
    {
        public GetSaleValidator()
        {
            RuleFor(x => x.SaleNumber)
                .NotEmpty()
                .Matches(@"^SALE-\d{8}-[A-F0-9]{8}$")
                .WithMessage("Invalid sale number format");
        }
    }
}
