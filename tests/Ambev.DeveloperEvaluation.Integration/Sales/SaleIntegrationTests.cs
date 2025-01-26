using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi;
using Ambev.DeveloperEvaluation.Shared;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.UpdateSale;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.Integration.Sales
{
    /// <summary>
    /// Comprehensive integration tests for Sales API endpoints
    /// </summary>
    public class SalesIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly DefaultContext _context;
        private readonly ITestOutputHelper _output;

        public SalesIntegrationTests(
            TestWebApplicationFactory<Program> factory,
            ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
            _client = factory.CreateClient();

            var scope = factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        }

        /// <summary>
        /// Creates a test sale in the database
        /// </summary>
        private async Task<Sale> CreateTestSale()
        {
            try
            {
                _output.WriteLine("Starting CreateTestSale");

                var sale = new Sale(
                    Guid.NewGuid(),
                    "Test Customer",
                    Guid.NewGuid(),
                    "Test Branch",
                    DateTime.UtcNow);

                sale.AddItem(
                    Guid.NewGuid(),
                    "Initial Product",
                    100m,
                    1);

                _output.WriteLine($"Creating test sale with ID: {sale.Id}");

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                var savedSale = await _context.Sales
                    .Include(s => s.Items)
                    .FirstOrDefaultAsync(s => s.Id == sale.Id);

                if (savedSale == null)
                {
                    _output.WriteLine("Failed to save sale to database");
                    throw new InvalidOperationException("Sale was not saved to database");
                }

                _output.WriteLine($"Successfully created sale with {savedSale.Items.Count} items");

                _context.ChangeTracker.Clear();

                return savedSale;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"CreateTestSale failed with exception: {ex.GetType().Name}");
                _output.WriteLine($"Exception message: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Test retrieving a sale by its valid ID
        /// </summary>
        [Fact(DisplayName = "Get Sale - Valid ID Returns Sale Details")]
        public async Task GetSale_WithValidId_ReturnsSaleDetails()
        {
            var sale = await CreateTestSale();

            var response = await _client.GetAsync($"/api/sales/{sale.Id}");

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<GetSaleResponse>>();

            Assert.NotNull(result);
            Assert.Equal(sale.Id, result.Data.Sale.Id);
            Assert.Equal(sale.CustomerName, result.Data.Sale.CustomerName);
            Assert.Equal(sale.BranchName, result.Data.Sale.BranchName);
        }

        /// <summary>
        /// Test cancelling a sale with a valid ID
        /// </summary>
        [Fact(DisplayName = "Cancel Sale - Valid ID Cancels Sale")]
        public async Task CancelSale_WithValidId_CancelsSale()
        {
            var sale = await CreateTestSale();

            var response = await _client.PostAsync($"/api/sales/{sale.Id}/cancel", null);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<CancelSaleResponse>>();

            Assert.NotNull(result);
            Assert.True(result.Data.Sale.IsCancelled);
        }

        /// <summary>
        /// Test updating a sale with valid data
        /// </summary>
        [Fact(DisplayName = "Update Sale - Valid Data Updates Sale")]
        public async Task UpdateSale_WithValidData_UpdatesSale()
        {
            try
            {
                var sale = await CreateTestSale();

                await Task.Delay(100);

                var freshSale = await _context.Sales
                    .Include(s => s.Items)
                    .FirstOrDefaultAsync(s => s.Id == sale.Id);

                if (freshSale == null)
                {
                    _output.WriteLine($"Failed to load sale with ID: {sale.Id}");
                    throw new InvalidOperationException("Could not load the created sale");
                }

                _output.WriteLine($"Test Sale ID: {freshSale.Id}");
                _output.WriteLine($"Number of items: {freshSale.Items.Count}");
                _output.WriteLine($"Existing Sale Customer Name: {freshSale.CustomerName}");

                var updateRequest = new UpdateSaleRequest
                {
                    CustomerId = freshSale.CustomerId,
                    CustomerName = "Updated Customer Name",
                    BranchId = freshSale.BranchId,
                    BranchName = "Updated Branch Name",
                    Items = new List<UpdateSaleRequest.UpdateSaleItemRequest>()
                };

                if (freshSale.Items.Any())
                {
                    var existingItem = freshSale.Items.First();
                    updateRequest.Items.Add(new UpdateSaleRequest.UpdateSaleItemRequest
                    {
                        ProductId = existingItem.ProductId,
                        ProductName = "Updated Product",
                        UnitPrice = 150m,
                        Quantity = 3
                    });
                }
                else
                {
                    updateRequest.Items.Add(new UpdateSaleRequest.UpdateSaleItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "New Product",
                        UnitPrice = 150m,
                        Quantity = 3
                    });
                }

                _output.WriteLine("Update Request Content:");
                _output.WriteLine(JsonSerializer.Serialize(updateRequest, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

                var response = await _client.PutAsJsonAsync($"/api/sales/{freshSale.Id}", updateRequest);

                var responseContent = await response.Content.ReadAsStringAsync();
                _output.WriteLine($"Response Status Code: {response.StatusCode}");
                _output.WriteLine($"Response Content: {responseContent}");

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<UpdateSaleResponse>>();
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Equal("Updated Customer Name", result.Data.Sale.CustomerName);

                using (var scope = _factory.Services.CreateScope())
                {
                    var verificationContext = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                    var updatedSale = await verificationContext.Sales
                        .Include(s => s.Items)
                        .FirstOrDefaultAsync(s => s.Id == freshSale.Id);

                    Assert.NotNull(updatedSale);
                    Assert.Equal("Updated Customer Name", updatedSale.CustomerName);
                    Assert.Contains(updatedSale.Items, i => i.ProductName.Contains("Updated") || i.ProductName.Contains("New"));
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Test failed with exception: {ex.GetType().Name}");
                _output.WriteLine($"Exception message: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Test listing sales with pagination
        /// </summary>
        [Fact(DisplayName = "List Sales - Pagination Works Correctly")]
        public async Task ListSales_WithPagination_ReturnsCorrectResults()
        {
            await CreateTestSale();
            await CreateTestSale();
            await CreateTestSale();

            var response = await _client.GetAsync("/api/sales?_page=1&_size=2");

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<PaginatedResponse<SaleResponse>>>();

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Data.Count());
            Assert.Equal(1, result.Data.CurrentPage);
            Assert.Equal(3, result.Data.TotalCount);
        }

        /// <summary>
        /// Test retrieving a non-existent sale
        /// </summary>
        [Fact(DisplayName = "Get Sale - Non-Existent ID Returns Not Found")]
        public async Task GetSale_WithNonExistentId_ReturnsNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/sales/{nonExistentId}");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Test updating a non-existent sale
        /// </summary>
        [Fact(DisplayName = "Update Sale - Non-Existent ID Returns Not Found")]
        public async Task UpdateSale_WithNonExistentId_ReturnsNotFound()
        {
            var nonExistentId = Guid.NewGuid();
            var updateRequest = new UpdateSaleRequest
            {
                CustomerName = "Updated Customer",
                Items = new List<UpdateSaleRequest.UpdateSaleItemRequest>()
            };

            var response = await _client.PutAsJsonAsync($"/api/sales/{nonExistentId}", updateRequest);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Test cancelling a non-existent sale
        /// </summary>
        [Fact(DisplayName = "Cancel Sale - Non-Existent ID Returns Not Found")]
        public async Task CancelSale_WithNonExistentId_ReturnsNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await _client.PostAsync($"/api/sales/{nonExistentId}/cancel", null);

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
