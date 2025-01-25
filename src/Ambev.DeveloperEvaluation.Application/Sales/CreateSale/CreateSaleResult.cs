using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public class CreateSaleResult
    {
        public string SaleNumber { get; set; }
        public SaleDto Sale { get; set; }
    }
}
