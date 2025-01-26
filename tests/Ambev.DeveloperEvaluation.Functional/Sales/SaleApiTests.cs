using Ambev.DeveloperEvaluation.WebApi;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.GetSale;
using Ambev.DeveloperEvaluation.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using System.Text.Json;
using System.Text;
using Xunit.Abstractions;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace Ambev.DeveloperEvaluation.Functional.Sales
{
    /// <summary>
    /// Functional tests for the Sales API endpoints
    /// </summary>
    public class SalesApiTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public SalesApiTests(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
        private async Task<Guid> CreateTestSale(HttpClient client)
        {
            var request = new CreateSaleRequest
            {
                CustomerId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                BranchId = Guid.NewGuid(),
                BranchName = "Test Branch",
                SaleDate = DateTime.UtcNow,
                Items = new List<CreateSaleRequest.CreateSaleItemRequest>
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

            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/sales", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponseWithData<CreateSaleResponse>>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result?.Data?.Sale?.Id ?? throw new InvalidOperationException("Failed to create test sale");
        }

        private async Task<Guid> CreateTestSaleWithAmount(HttpClient client, decimal amount)
        {
            var request = new CreateSaleRequest
            {
                CustomerId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                BranchId = Guid.NewGuid(),
                BranchName = "Test Branch",
                SaleDate = DateTime.UtcNow,
                Items = new List<CreateSaleRequest.CreateSaleItemRequest>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Test Product",
                        UnitPrice = amount,
                        Quantity = 1
                    }
                }
            };

            var response = await client.PostAsJsonAsync("/api/sales", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>();
            return result.Data.Sale.Id;
        }

        [Fact]
        public async Task GetSale_WithValidId_ShouldReturnSaleDetails()
        {
            // Arrange
            var saleId = await CreateTestSale(_client);

            // Act
            var response = await _client.GetAsync($"/api/sales/{saleId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<GetSaleResponse>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(saleId, result.Data.Sale.Id);
        }

        [Fact]
        public async Task ListSales_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange

            // Create multiple test sales
            for (int i = 0; i < 15; i++)
            {
                await CreateTestSale(_client);
            }

            // Act
            var response = await _client.GetAsync("/api/sales?_page=2&_size=5");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<PaginatedResponse<SaleResponse>>>();

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.CurrentPage);
            Assert.Equal(5, result.Data.Data.Count());
        }

        [Fact]
        public async Task CreateSale_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateSaleRequest
            {
                CustomerId = Guid.Empty,
                CustomerName = "",
                Items = new List<CreateSaleRequest.CreateSaleItemRequest>()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/sales", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "Given existing sale When cancelling Then returns success")]
        public async Task CancelSale_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var saleId = await CreateTestSale(_client);
            _output.WriteLine("SaleId" + saleId);

            // Act
            var response = await _client.PostAsync($"/api/sales/{saleId}/cancel", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<CancelSaleResponse>>();
            Assert.True(result.Success);
            Assert.True(result.Data.Sale.IsCancelled);

        }

        [Fact(DisplayName = "Given sale search When filtering by date range Then returns correct sales")]
        public async Task ListSales_WithDateFilter_ReturnsFilteredSales()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-5);
            var endDate = DateTime.UtcNow;

            // Create test sales
            await CreateTestSale(_client);
            await CreateTestSale(_client);

            // Act
            var response = await _client.GetAsync($"/api/sales?_minDate={startDate:yyyy-MM-dd}&_maxDate={endDate:yyyy-MM-dd}");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<PaginatedResponse<SaleResponse>>>();
            Assert.NotNull(result);
            Assert.All(result.Data.Data, sale =>
                Assert.True(sale.SaleDate >= startDate && sale.SaleDate <= endDate));
        }

        [Fact(DisplayName = "Given sale search When ordering by multiple fields Then returns correctly ordered sales")]
        public async Task ListSales_WithMultipleOrdering_ReturnsOrderedSales()
        {
            // Create test sales with different dates and amounts
            await CreateTestSaleWithAmount(_client, 100m);
            await CreateTestSaleWithAmount(_client, 200m);
            await CreateTestSaleWithAmount(_client, 150m);

            // Act
            var response = await _client.GetAsync("/api/sales?_order=totalAmount desc,saleDate asc");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<PaginatedResponse<SaleResponse>>>();
            Assert.NotNull(result);

            var sales = result.Data.Data.ToList();
            for (int i = 1; i < sales.Count; i++)
            {
                Assert.True(sales[i - 1].TotalAmount >= sales[i].TotalAmount);
            }
        }

        [Fact]
        public async Task ListSales_WithFilters_ReturnsFilteredResults()
        {
            // Create test sales
            await CreateTestSale(_client);

            // Test string filter
            var response = await _client.GetAsync("/api/sales?customerName=Test Customer");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<PaginatedResponse<SaleResponse>>>();

            Assert.NotNull(result);
            Assert.All(result.Data.Data, sale =>
                Assert.StartsWith("Test Customer", sale.CustomerName));
        }

        [Fact]
        public async Task ListSales_WithRangeFilter_ReturnsFilteredResults()
        {
            // Create test sales with different amounts
            await CreateTestSaleWithAmount(_client, 50m);
            await CreateTestSaleWithAmount(_client, 150m);
            await CreateTestSaleWithAmount(_client, 250m);

            // Test range filter
            var response = await _client.GetAsync("/api/sales?_minTotalAmount=100&_maxTotalAmount=200");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<PaginatedResponse<SaleResponse>>>();

            Assert.NotNull(result);
            Assert.All(result.Data.Data, sale =>
                Assert.True(sale.TotalAmount >= 100m && sale.TotalAmount <= 200m));
        }

        [Fact]
        public async Task ListSales_WithInvalidFilter_ReturnsBadRequest()
        {
            // Test invalid filter
            var response = await _client.GetAsync("/api/sales?invalidField=value");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
