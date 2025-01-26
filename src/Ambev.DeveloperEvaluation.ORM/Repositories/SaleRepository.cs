using Ambev.DeveloperEvaluation.Common.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories
{
    /// <summary>
    /// Repository implementation for Sale entity operations using GUID identifiers
    /// </summary>
    public class SaleRepository : ISaleRepository
    {
        private readonly DefaultContext _context;

        public SaleRepository(DefaultContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a sale by its GUID identifier
        /// </summary>
        public async Task<Sale> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        /// <summary>
        /// Retrieves a sale by its unique sale number
        /// </summary>
        public async Task<Sale> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
        }

        /// <summary>
        /// Retrieves a paginated list of sales with optional ordering
        /// </summary>
        public async Task<IEnumerable<Sale>> GetAllAsync(
            int page,
            int size,
            string orderBy,
            Dictionary<string, string> filters,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Sales
                .Include(s => s.Items)
                .AsNoTracking();

            foreach (var filter in filters)
            {
                // Handle range filters
                if (filter.Key.StartsWith("_min") || filter.Key.StartsWith("_max"))
                {
                    query = ApplyRangeFilter(query, filter);
                    continue;
                }

                // Handle string filters with wildcards
                if (filter.Value.Contains("*"))
                {
                    query = ApplyWildcardFilter(query, filter);
                    continue;
                }

                // Handle exact match filters
                query = ApplyExactFilter(query, filter);
            }


            // Apply ordering if specified
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = ApplyOrdering(query, orderBy);
            }
            else
            {
                // Default ordering by sale date descending
                query = query.OrderByDescending(s => s.SaleDate);
            }

            return await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds a new sale to the database
        /// </summary>
        public async Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync(cancellationToken);
            return sale;
        }

        /// <summary>
        /// Updates an existing sale in the database
        /// </summary>
        public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            _context.ChangeTracker.Clear(); // Clear existing tracking

            // Attach the sale with all its navigation properties
            _context.Sales.Attach(sale);
            _context.Entry(sale).State = EntityState.Modified;

            // Carefully manage sale items
            foreach (var item in sale.Items)
            {
                // Determine the appropriate state for each item
                var existingItem = _context.SaleItems.Find(item.Id);

                if (existingItem == null)
                {
                    // New item needs to be added
                    _context.SaleItems.Add(item);
                }
                else
                {
                    // Existing item needs to be updated
                    _context.Entry(item).State = EntityState.Modified;
                }
            }

            // Remove any items that are no longer part of the sale
            var currentItemIds = sale.Items.Select(i => i.Id).ToHashSet();
            var itemsToRemove = _context.SaleItems
                .Where(i => i.SaleId == sale.Id && !currentItemIds.Contains(i.Id))
                .ToList();

            _context.SaleItems.RemoveRange(itemsToRemove);

            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the total count of sales in the database
        /// </summary>
        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Sales.CountAsync(cancellationToken);
        }

        private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string orderBy)
        {
            // If no ordering specified, return with default ordering
            if (string.IsNullOrWhiteSpace(orderBy))
                return query.OrderByDescending(s => s.SaleDate);

            // Split the order string into individual ordering expressions
            var orderClauses = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var firstOrderApplied = false;
            IOrderedQueryable<Sale> orderedQuery = null;

            foreach (var clause in orderClauses)
            {
                // Parse each ordering clause
                var trimmedClause = clause.Trim();
                var descending = trimmedClause.EndsWith(" desc", StringComparison.OrdinalIgnoreCase);

                // Get the property name by removing the ordering direction
                var propertyName = descending
                    ? trimmedClause[..^5].Trim()
                    : trimmedClause.EndsWith(" asc", StringComparison.OrdinalIgnoreCase)
                        ? trimmedClause[..^4].Trim()
                        : trimmedClause;

                // For the first ordering, we use OrderBy/OrderByDescending
                // For subsequent orderings, we use ThenBy/ThenByDescending
                if (!firstOrderApplied)
                {
                    orderedQuery = ApplyInitialOrdering(query, propertyName, descending);
                    firstOrderApplied = true;
                }
                else
                {
                    orderedQuery = ApplyThenOrdering(orderedQuery, propertyName, descending);
                }
            }

            // If no valid ordering was applied, return the query with default ordering
            return orderedQuery ?? query.OrderByDescending(s => s.SaleDate);
        }

        private static IOrderedQueryable<Sale> ApplyInitialOrdering(IQueryable<Sale> query, string propertyName, bool descending)
        {
            return propertyName.ToLower() switch
            {
                "date" or "saledate" => descending
                    ? query.OrderByDescending(s => s.SaleDate)
                    : query.OrderBy(s => s.SaleDate),

                "amount" or "totalamount" => descending
                    ? query.OrderByDescending(s => s.TotalAmount)
                    : query.OrderBy(s => s.TotalAmount),

                "customer" or "customername" => descending
                    ? query.OrderByDescending(s => s.CustomerName)
                    : query.OrderBy(s => s.CustomerName),

                "branch" or "branchname" => descending
                    ? query.OrderByDescending(s => s.BranchName)
                    : query.OrderBy(s => s.BranchName),

                "number" or "salenumber" => descending
                    ? query.OrderByDescending(s => s.SaleNumber)
                    : query.OrderBy(s => s.SaleNumber),

                _ => query.OrderByDescending(s => s.SaleDate) // Default ordering if property not recognized
            };
        }

        private static IOrderedQueryable<Sale> ApplyThenOrdering(IOrderedQueryable<Sale> query, string propertyName, bool descending)
        {
            return propertyName.ToLower() switch
            {
                "date" or "saledate" => descending
                    ? query.ThenByDescending(s => s.SaleDate)
                    : query.ThenBy(s => s.SaleDate),

                "amount" or "totalamount" => descending
                    ? query.ThenByDescending(s => s.TotalAmount)
                    : query.ThenBy(s => s.TotalAmount),

                "customer" or "customername" => descending
                    ? query.ThenByDescending(s => s.CustomerName)
                    : query.ThenBy(s => s.CustomerName),

                "branch" or "branchname" => descending
                    ? query.ThenByDescending(s => s.BranchName)
                    : query.ThenBy(s => s.BranchName),

                "number" or "salenumber" => descending
                    ? query.ThenByDescending(s => s.SaleNumber)
                    : query.ThenBy(s => s.SaleNumber),

                _ => query // If property not recognized, maintain existing ordering
            };
        }

        private IQueryable<Sale> ApplyRangeFilter(IQueryable<Sale> query, KeyValuePair<string, string> filter)
        {
            var fieldName = filter.Key[4..]; // Remove "_min" or "_max"
            var value = filter.Value;

            return fieldName.ToLower() switch
            {
                "totalamount" when decimal.TryParse(value, out var amount) =>
                    filter.Key.StartsWith("_min")
                        ? query.Where(s => s.TotalAmount >= amount)
                        : query.Where(s => s.TotalAmount <= amount),

                "date" when DateTime.TryParse(value, out var date) =>
                    filter.Key.StartsWith("_min")
                        ? query.Where(s => s.SaleDate >= date)
                        : query.Where(s => s.SaleDate <= date),

                _ => throw new ValidationEx($"Invalid range filter field: {fieldName}")
            };
        }

        private IQueryable<Sale> ApplyWildcardFilter(IQueryable<Sale> query, KeyValuePair<string, string> filter)
        {
            var value = filter.Value.Replace("*", "");

            return filter.Key.ToLower() switch
            {
                "customername" when filter.Value.StartsWith("*") =>
                    query.Where(s => s.CustomerName.EndsWith(value)),
                "customername" =>
                    query.Where(s => s.CustomerName.StartsWith(value)),

                "branchname" when filter.Value.StartsWith("*") =>
                    query.Where(s => s.BranchName.EndsWith(value)),
                "branchname" =>
                    query.Where(s => s.BranchName.StartsWith(value)),

                _ => throw new ValidationEx($"Invalid wildcard filter field: {filter.Key}")
            };
        }

        private IQueryable<Sale> ApplyExactFilter(IQueryable<Sale> query, KeyValuePair<string, string> filter)
        {
            return filter.Key.ToLower() switch
            {
                "customername" => query.Where(s => s.CustomerName == filter.Value),
                "branchname" => query.Where(s => s.BranchName == filter.Value),
                "iscancelled" when bool.TryParse(filter.Value, out var cancelled) =>
                    query.Where(s => s.IsCancelled == cancelled),
                _ => throw new ValidationEx($"Invalid filter field: {filter.Key}")
            };
        }
    }
}
