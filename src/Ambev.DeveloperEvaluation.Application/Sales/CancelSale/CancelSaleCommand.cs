using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale
{
    public class CancelSaleCommand : IRequest<CancelSaleResult>
    {
        public string SaleNumber { get; set; }
    }
}
