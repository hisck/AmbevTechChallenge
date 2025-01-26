using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale
{
    /// <summary>
    /// Controller for managing sale operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of SalesController
        /// </summary>
        /// <param name="mediator">The mediator instance</param>
        /// <param name="mapper">The AutoMapper instance</param>
        public SalesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new sale
        /// </summary>
        /// <param name="request">The sale creation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created sale details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateSaleRequestValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var command = _mapper.Map<CreateSaleCommand>(request);
            var response = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(
                actionName: nameof(GetSale),
                controllerName: "Sales",
                routeValues: new { id = response.Sale.Id },
                value: new ApiResponseWithData<CreateSaleResponse>
                {
                    Success = true,
                    Message = "Sale created successfully",
                    Data = _mapper.Map<CreateSaleResponse>(response)
                }
            );
        }

        /// <summary>
        /// Retrieves a sale by ID
        /// </summary>
        /// <param name="id">The unique identifier of the sale</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The sale details if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var request = new GetSaleRequest { Id = id };
            var validator = new GetSaleRequestValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var command = _mapper.Map<GetSaleCommand>(request.Id);
            var response = await _mediator.Send(command, cancellationToken);

            return Ok(_mapper.Map<GetSaleResponse>(response));
        }

        /// <summary>
        /// Updates an existing sale
        /// </summary>
        /// <param name="id">The unique identifier of the sale to update</param>
        /// <param name="request">The sale update request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated sale details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSale(
            [FromRoute] Guid id,
            [FromBody] UpdateSaleRequest request,
            CancellationToken cancellationToken)
        {
            var validator = new UpdateSaleRequestValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var command = _mapper.Map<UpdateSaleCommand>(request);
            command.Id = id;

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(_mapper.Map<UpdateSaleResponse>(result));
        }

        /// <summary>
        /// Lists all sales with pagination and ordering
        /// </summary>
        /// <param name="request">The list sales request containing pagination and ordering parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A paginated list of sales</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<SaleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ListSales(
            [FromQuery] ListSalesRequest request,
            CancellationToken cancellationToken)
        {
            var validator = new ListSalesRequestValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var command = _mapper.Map<ListSalesCommand>(request);
            var filters = new Dictionary<string, string>();
            foreach (var query in Request.Query)
            {
                if (!query.Key.StartsWith("_"))
                {
                    filters.Add(query.Key, query.Value.ToString());
                }
            }
            command.Filters = filters;
            var result = await _mediator.Send(command, cancellationToken);

            return OkPaginated(
                new PaginatedList<SaleResponse>(
                    _mapper.Map<List<SaleResponse>>(result.Sales),
                    result.TotalCount,
                    result.Page,
                    result.PageSize
                ));
        }

        /// <summary>
        /// Cancels a sale by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the sale to cancel</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The cancelled sale information</returns>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelSale(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var request = new CancelSaleRequest { Id = id };
            var validator = new CancelSaleRequestValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var command = _mapper.Map<CancelSaleCommand>(request);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(_mapper.Map<CancelSaleResponse>(result));
        }
    }
}
