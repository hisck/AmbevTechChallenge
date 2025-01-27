using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories
{
    public interface ISaleRepository
    {
        Task<Sale> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Sale> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Sale>> GetAllAsync(int page, int size, string OrderBy, Dictionary<string, string> filters, CancellationToken cancellationToken = default);
        Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default);
        Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(Dictionary<string, string> filters, CancellationToken cancellationToken = default);
    }
}
