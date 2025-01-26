using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales
{
    public class ListSalesCommand : IRequest<ListSalesResult>
    {
        public int _page { get; set; } = 1;
        public int _size { get; set; } = 10;
        public string _order { get; set; } = string.Empty;
        public Dictionary<string, string> Filters { get; set; } = new();
    }
}
