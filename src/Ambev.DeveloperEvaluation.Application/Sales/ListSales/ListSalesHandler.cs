using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales
{
    public class ListSalesHandler : IRequestHandler<ListSalesCommand, ListSalesResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;

        public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
        }

        public async Task<ListSalesResult> Handle(ListSalesCommand request, CancellationToken cancellationToken)
        {
            var sales = await _saleRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
            var totalCount = await _saleRepository.GetTotalCountAsync(cancellationToken);

            return new ListSalesResult
            {
                Sales = _mapper.Map<List<SaleDto>>(sales),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
