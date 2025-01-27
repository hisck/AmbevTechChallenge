using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Common.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application
{
    public class ListSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        private readonly ListSalesHandler _handler;

        public ListSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mapper = Substitute.For<IMapper>();
            _handler = new ListSalesHandler(_saleRepository, _mapper);
        }

        [Fact(DisplayName = "Given valid request When listing sales Then returns paginated results")]
        public async Task Handle_ValidRequest_ReturnsPaginatedResults()
        {
            var query = new ListSalesCommand
            {
                _page = 1,
                _size = 10,
                Filters = new Dictionary<string, string>()
            };

            var sales = new List<Sale>
        {
            new(Guid.NewGuid(), "Customer 1", Guid.NewGuid(), "Branch 1", DateTime.UtcNow),
            new(Guid.NewGuid(), "Customer 2", Guid.NewGuid(), "Branch 2", DateTime.UtcNow)
        };

            _saleRepository.GetAllAsync(1, 10, null, Arg.Any<Dictionary<string, string>>())
                .Returns(sales);
            _saleRepository.GetTotalCountAsync(Arg.Any<Dictionary<string, string>>()).Returns(2);
            _mapper.Map<IEnumerable<SaleDto>>(sales)
                .Returns(sales.Select(s => new SaleDto()));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
        }

        [Fact(DisplayName = "Given invalid filter When listing sales Then throws ValidationException")]
        public async Task Handle_InvalidFilter_ThrowsValidationException()
        {
            var query = new ListSalesCommand
            {
                Filters = new Dictionary<string, string> { { "invalidField", "value" } }
            };

            _saleRepository.When(x =>
            x.GetAllAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<Dictionary<string, string>>()))
            .Do(x => { throw new ValidationEx("Invalid filter field: invalidField"); });

            await Assert.ThrowsAsync<ValidationEx>(() =>
                _handler.Handle(query, CancellationToken.None));
        }
    }
}
