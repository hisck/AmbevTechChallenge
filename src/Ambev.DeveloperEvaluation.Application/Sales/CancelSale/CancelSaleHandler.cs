using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale
{
    public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;

        public CancelSaleHandler(ISaleRepository saleRepository, IMapper mapper)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
        }

        public async Task<CancelSaleResult> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetBySaleNumberAsync(request.SaleNumber, cancellationToken);

            if (sale == null)
                throw new KeyNotFoundException($"Sale {request.SaleNumber} not found");

            sale.Cancel();
            await _saleRepository.UpdateAsync(sale, cancellationToken);

            return new CancelSaleResult
            {
                Sale = _mapper.Map<SaleDto>(sale)
            };
        }
    }
}
