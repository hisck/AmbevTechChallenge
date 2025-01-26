using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Common.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application
{
    public class GetSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        private readonly GetSaleHandler _handler;

        public GetSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mapper = Substitute.For<IMapper>();
            _handler = new GetSaleHandler(_saleRepository, _mapper);
        }

        /// <summary>
        /// Helper method to create a test sale with predictable properties
        /// </summary>
        private Sale CreateTestSale(Guid saleId)
        {
            var sale = new Sale(
                Guid.NewGuid(),
                "Test Customer",
                Guid.NewGuid(),
                "Test Branch",
                DateTime.UtcNow
            );

            var idProperty = typeof(Sale).GetProperty("Id");
            idProperty?.SetValue(sale, saleId);

            sale.AddItem(
                Guid.NewGuid(),
                "Test Product",
                100m,
                2
            );

            return sale;
        }

        [Fact(DisplayName = "GetSaleHandler - Existing Sale - Returns Sale Details")]
        public async Task Handle_ExistingSale_ReturnsSaleDetails()
        {
            var saleId = Guid.NewGuid();
            var sale = CreateTestSale(saleId);

            _saleRepository
                .GetByIdAsync(saleId, Arg.Any<CancellationToken>())
                .Returns(sale);

            var expectedSaleDto = new SaleDto
            {
                Id = sale.Id,
                CustomerName = sale.CustomerName,
                BranchName = sale.BranchName,
                BranchId = sale.BranchId,
                CustomerId = sale.CustomerId,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                IsCancelled = sale.IsCancelled
            };

            _mapper
                .Map<SaleDto>(sale)
                .Returns(expectedSaleDto);

            var query = new GetSaleCommand { Id = saleId };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Sale);

            Assert.Equal(sale.Id, result.Sale.Id);
            Assert.Equal(sale.CustomerName, result.Sale.CustomerName);
            Assert.Equal(sale.BranchName, result.Sale.BranchName);
            Assert.Equal(sale.BranchId, result.Sale.BranchId);
            Assert.Equal(sale.CustomerId, result.Sale.CustomerId);
            Assert.Equal(sale.SaleDate, result.Sale.SaleDate);
            Assert.Equal(sale.TotalAmount, result.Sale.TotalAmount);
            Assert.Equal(sale.IsCancelled, result.Sale.IsCancelled);

            await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());
            _mapper.Received(1).Map<SaleDto>(Arg.Is<Sale>(s => s.Id == saleId));
        }

        [Fact(DisplayName = "GetSaleHandler - Non-Existing Sale - Throws ResourceNotFoundException")]
        public async Task Handle_NonExistingSale_ThrowsResourceNotFoundException()
        {
            var saleId = Guid.NewGuid();

            _saleRepository
                .GetByIdAsync(saleId, Arg.Any<CancellationToken>())
                .Returns((Sale)null);

            var query = new GetSaleCommand { Id = saleId };

            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
                () => _handler.Handle(query, CancellationToken.None)
            );

            Assert.Equal($"Sale with Id {saleId} not found", exception.Message);

            await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());
            _mapper.DidNotReceive().Map<SaleDto>(Arg.Any<Sale>());
        }
    }
}
