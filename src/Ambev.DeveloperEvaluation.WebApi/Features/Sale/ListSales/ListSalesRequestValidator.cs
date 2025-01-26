using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.ListSales
{
    /// <summary>
    /// Validator for ListSalesRequest
    /// </summary>
    public class ListSalesRequestValidator : AbstractValidator<ListSalesRequest>
    {
        /// <summary>
        /// Initializes validation rules for ListSalesRequest
        /// </summary>
        public ListSalesRequestValidator()
        {
            RuleFor(x => x._page)
                .GreaterThan(0)
                .When(x => x._page.HasValue)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x._size)
                .InclusiveBetween(1, 100)
                .When(x => x._size.HasValue)
                .WithMessage("Page size must be between 1 and 100");
        }
    }
}
