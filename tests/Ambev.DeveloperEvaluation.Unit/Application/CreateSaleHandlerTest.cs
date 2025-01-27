using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Common.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _eventPublisher);
    }

    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = new Sale(
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName,
            command.SaleDate
        );

        foreach (var item in command.Items)
        {
            sale.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        var result = new CreateSaleResult
        {
            SaleNumber = sale.SaleNumber,
            Sale = new SaleDto
            {
                Id = sale.Id,
                SaleNumber = sale.SaleNumber,
                SaleDate = sale.SaleDate,
                CustomerId = sale.CustomerId,
                CustomerName = sale.CustomerName,
                BranchId = sale.BranchId,
                BranchName = sale.BranchName,
                TotalAmount = sale.TotalAmount,
                IsCancelled = sale.IsCancelled
            }
        };

        _saleRepository.AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(result);

        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        createSaleResult.Should().NotBeNull();
        createSaleResult.SaleNumber.Should()
            .MatchRegex(@"^SALE-\d{8}-[A-F0-9]{8}$",
            "Sale number should follow expected format");

        createSaleResult.SaleNumber.Should().StartWith("SALE-");
        createSaleResult.SaleNumber.Should().Contain(DateTime.UtcNow.ToString("yyyyMMdd"));
        await _saleRepository.Received(1).AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid sale data When creating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var command = new CreateSaleCommand();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given sale with items quantity above 20 When creating sale Then throws fluent validation exception")]
    public async Task Handle_ItemQuantityAbove20_ThrowsFluentException()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items.First().Quantity = 21;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Quantity must be between 1 and 20 items*");
    }
}

public static class CreateSaleHandlerTestData
{
    public static CreateSaleCommand GenerateValidCommand()
    {
        return new CreateSaleCommand
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch",
            SaleDate = DateTime.UtcNow,
            Items = new List<CreateSaleCommand.CreateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    UnitPrice = 100m,
                    Quantity = 1
                }
            }
        };
    }
}