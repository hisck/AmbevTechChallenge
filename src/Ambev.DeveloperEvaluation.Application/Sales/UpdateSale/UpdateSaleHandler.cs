using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Common.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Common.DTOs;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
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
            var validator = new UpdateSaleValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var existingSale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingSale == null)
                throw new ResourceNotFoundException($"Sale with Id {request.Id} not found");

            // Transform request items to DTO
            var updateItems = request.Items.Select(i => new UpdateSaleItemDto(
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity
            )).ToList();

            // Use new comprehensive update method
            existingSale.UpdateSaleDetails(
                request.CustomerId,
                request.CustomerName,
                request.BranchId,
                request.BranchName,
                updateItems
            );

            await _saleRepository.UpdateAsync(existingSale, cancellationToken);

            return new UpdateSaleResult
            {
                Sale = _mapper.Map<SaleDto>(existingSale)
            };
        }
    }
}
