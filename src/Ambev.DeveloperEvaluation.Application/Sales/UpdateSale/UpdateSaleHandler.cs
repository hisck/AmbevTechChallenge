using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale
{
    public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;

        public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
        }

        public async Task<UpdateSaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
        {
            var existingSale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingSale == null)
                throw new KeyNotFoundException($"Sale with Id {request.Id} not found");

            // Create new sale with updated information
            var updatedSale = new Sale(
                request.CustomerId,
                request.CustomerName,
                request.BranchId,
                request.BranchName,
                request.SaleDate);

            // Add items
            foreach (var item in request.Items)
            {
                updatedSale.AddItem(
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity);
            }

            await _saleRepository.UpdateAsync(updatedSale, cancellationToken);

            return new UpdateSaleResult
            {
                Sale = _mapper.Map<SaleDto>(updatedSale)
            };
        }
    }
}
