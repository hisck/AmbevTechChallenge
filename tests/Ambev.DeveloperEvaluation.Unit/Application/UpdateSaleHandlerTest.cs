using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Common.Events;
using Ambev.DeveloperEvaluation.Common.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application
{
    public class UpdateSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly UpdateSaleHandler _handler;

        public UpdateSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mapper = Substitute.For<IMapper>();
            _eventPublisher = Substitute.For<IEventPublisher>();
            _handler = new UpdateSaleHandler(_saleRepository, _mapper, _eventPublisher);
        }

        [Fact(DisplayName = "Given valid update request When updating sale Then returns updated sale")]
        public async Task Handle_ValidRequest_ReturnsUpdatedSale()
        {
            var saleId = Guid.NewGuid();
            var command = new UpdateSaleCommand
            {
                Id = saleId,
                CustomerName = "Updated Customer",
                CustomerId = Guid.NewGuid(),
                BranchId = Guid.NewGuid(),
                BranchName = "Updated Branch",
                Items = new List<UpdateSaleCommand.UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    UnitPrice = 100m,
                    Quantity = 5
                }
            }
            };

            var existingSale = new Sale(
                command.CustomerId,
                "Original Customer",
                command.BranchId,
                "Original Branch",
                DateTime.UtcNow);

            _saleRepository.GetByIdAsync(saleId).Returns(existingSale);
            _mapper.Map<UpdateSaleResult>(Arg.Any<Sale>())
                .Returns(new UpdateSaleResult { Sale = new SaleDto() });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        }

        [Fact(DisplayName = "Given non-existent sale When updating Then throws ResourceNotFoundException")]
        public async Task Handle_NonExistentSale_ThrowsNotFoundException()
        {
            var command = new UpdateSaleCommand { 
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerName = "Test Name",
                BranchId = Guid.NewGuid(),
                BranchName = "Test Branch",
                Items = new List<UpdateSaleCommand.UpdateSaleItemCommand>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Test Product",
                        UnitPrice = 100m,
                        Quantity = 19
                    }
                }
            };
            _saleRepository.GetByIdAsync(command.Id).Returns((Sale)null);

            await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact(DisplayName = "Given invalid quantity When updating Then throws FluentValidation.ValidationException")]
        public async Task Handle_InvalidQuantity_ThrowsBusinessRuleException()
        {
            var command = new UpdateSaleCommand
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerName = "Test Name",
                BranchId = Guid.NewGuid(),
                BranchName = "Test Branch",
                Items = new List<UpdateSaleCommand.UpdateSaleItemCommand>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Test Product",
                        UnitPrice = 100m,
                        Quantity = 21
                    }
                }
            };

            var existingSale = new Sale(
                command.CustomerId,
                "Test Customer",
                command.BranchId,
                "Test Branch",
                DateTime.UtcNow);

            _saleRepository.GetByIdAsync(command.Id).Returns(existingSale);

            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}
