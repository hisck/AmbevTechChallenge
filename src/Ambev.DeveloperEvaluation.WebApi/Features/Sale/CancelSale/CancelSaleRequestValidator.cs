using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.CancelSale
{
    /// <summary>
    /// Validator for CancelSaleRequest
    /// </summary>
    public class CancelSaleRequestValidator : AbstractValidator<CancelSaleRequest>
    {
        /// <summary>
        /// Initializes validation rules for CancelSaleRequest
        /// </summary>
        public CancelSaleRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Valid sale ID is required");
        }
    }
}
