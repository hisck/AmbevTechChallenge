using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

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
            var validator = new ListSalesValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var sales = await _saleRepository.GetAllAsync(request._page, request._size, request._order, request.Filters, cancellationToken);

            var totalCount = await _saleRepository.GetTotalCountAsync(request.Filters, cancellationToken);

            return new ListSalesResult
            {
                Sales = _mapper.Map<List<SaleDto>>(sales),
                TotalCount = totalCount,
                Page = request._page,
                PageSize = request._size
            };
        }
    }
}
