using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.UpdateSale
{
    /// <summary>
    /// Validator for UpdateSaleRequest
    /// </summary>
    public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
    {
        /// <summary>
        /// Initializes validation rules for UpdateSaleRequest
        /// </summary>
        public UpdateSaleRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("Valid customer ID is required");

            RuleFor(x => x.CustomerName)
                .NotEmpty()
                .WithMessage("Customer name is required");

            RuleFor(x => x.BranchId)
                .NotEmpty()
                .WithMessage("Valid branch ID is required");

            RuleFor(x => x.BranchName)
                .NotEmpty()
                .WithMessage("Branch name is required");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("At least one item is required");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty()
                    .WithMessage("Valid product ID is required");

                item.RuleFor(x => x.ProductName)
                    .NotEmpty()
                    .WithMessage("Product name is required");

                item.RuleFor(x => x.UnitPrice)
                    .GreaterThan(0)
                    .WithMessage("Unit price must be greater than zero");

                item.RuleFor(x => x.Quantity)
                    .InclusiveBetween(1, 20)
                    .WithMessage("Quantity must be between 1 and 20 items");
            });
        }
    }
}
