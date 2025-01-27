using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using AutoMapper;
using FluentValidation;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Common.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Common.Events;

namespace Ambev.DeveloperEvaluation.Tests.Application.Sales
{
    public class CancelSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        private readonly CancelSaleHandler _handler;
        private readonly IEventPublisher _eventPublisher;

        public CancelSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mapper = Substitute.For<IMapper>();
            _eventPublisher = Substitute.For<IEventPublisher>();
            _handler = new CancelSaleHandler(_saleRepository, _mapper, _eventPublisher);
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

        [Fact(DisplayName = "CancelSaleHandler - Valid Sale - Successfully Cancels Sale")]
        public async Task Handle_ValidSale_ReturnsCancelledSale()
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
                IsCancelled = true
            };

            _mapper
                .Map<SaleDto>(sale)
                .Returns(expectedSaleDto);

            var command = new CancelSaleCommand { Id = saleId };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Sale);
            Assert.True(result.Sale.IsCancelled);

            await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());
            await _saleRepository.Received(1).UpdateAsync(
                Arg.Is<Sale>(s => s.Id == saleId && s.IsCancelled),
                Arg.Any<CancellationToken>()
            );
            _mapper.Received(1).Map<SaleDto>(Arg.Is<Sale>(s => s.Id == saleId && s.IsCancelled));
        }

        [Fact(DisplayName = "CancelSaleHandler - Non-Existing Sale - Throws ResourceNotFoundException")]
        public async Task Handle_NonExistentSale_ThrowsResourceNotFoundException()
        {
            var saleId = Guid.NewGuid();

            _saleRepository
                .GetByIdAsync(saleId, Arg.Any<CancellationToken>())
                .Returns((Sale)null);

            var command = new CancelSaleCommand { Id = saleId };

            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Equal($"Sale {saleId} not found", exception.Message);

            await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());
            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
            _mapper.DidNotReceive().Map<SaleDto>(Arg.Any<Sale>());
        }

        [Fact(DisplayName = "CancelSaleHandler - Invalid Command - Throws ValidationException")]
        public async Task Handle_InvalidCommand_ThrowsValidationException()
        {
            var command = new CancelSaleCommand { Id = Guid.Empty };

            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Contains(
                exception.Errors,
                error => error.ErrorMessage.Contains("'Id' deve ser informado.")
            );

 
            await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
            _mapper.DidNotReceive().Map<SaleDto>(Arg.Any<Sale>());
        }

        [Fact(DisplayName = "CancelSaleHandler - Already Cancelled Sale - Throws exception")]
        public async Task Handle_AlreadyCancelledSale_HandlesGracefully()
        {
            var saleId = Guid.NewGuid();
            var sale = CreateTestSale(saleId);
            sale.Cancel();

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
                IsCancelled = true
            };

            _mapper
                .Map<SaleDto>(sale)
                .Returns(expectedSaleDto);

            var command = new CancelSaleCommand { Id = saleId };

            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }
    }
}