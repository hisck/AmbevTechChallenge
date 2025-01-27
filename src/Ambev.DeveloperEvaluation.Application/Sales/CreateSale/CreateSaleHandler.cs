using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Common.Exceptions;
using Ambev.DeveloperEvaluation.Common.Events;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;

        public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IEventPublisher eventPublisher)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
        }

        public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateSaleValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var sale = new Sale(
                request.CustomerId,
                request.CustomerName,
                request.BranchId,
                request.BranchName,
                request.SaleDate);

            foreach (var item in request.Items)
            {
                if (item.Quantity > 20)
                {
                    throw new BusinessRuleException(
                        "Cannot add more than 20 items of the same product");
                }
                sale.AddItem(
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity);
            }

            await _saleRepository.AddAsync(sale, cancellationToken);

            await _eventPublisher.PublishAsync(new SaleCreatedEvent(sale));

            return new CreateSaleResult
            {
                SaleNumber = sale.SaleNumber,
                Sale = _mapper.Map<SaleDto>(sale)
            };
        }
    }
}
