using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;

        public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
        }

        public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = new Sale(
                request.CustomerId,
                request.CustomerName,
                request.BranchId,
                request.BranchName,
                request.SaleDate);

            foreach (var item in request.Items)
            {
                sale.AddItem(
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity);
            }

            await _saleRepository.AddAsync(sale, cancellationToken);

            return new CreateSaleResult
            {
                SaleNumber = sale.SaleNumber,
                Sale = _mapper.Map<SaleDto>(sale)
            };
        }
    }
}
