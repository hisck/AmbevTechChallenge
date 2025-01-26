using Ambev.DeveloperEvaluation.Common.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var errorResponse = exception switch
            {
                AppException appEx => new ErrorResponse
                {
                    Type = appEx.Type,
                    Error = GetUserFriendlyErrorMessage(appEx.Type),
                    Detail = appEx.Message
                },

                FluentValidation.ValidationException valEx => new ErrorResponse
                {
                    Type = "ValidationError",
                    Error = "One or more validation errors occurred",
                    Detail = string.Join("; ", valEx.Errors.Select(e => e.ErrorMessage))
                },
                
                DomainException domainEx => new ErrorResponse
                {
                    Type = "DomainException",
                    Error = "A business rule was violated",
                    Detail = domainEx.Message
                },

                _ => new ErrorResponse
                {
                    Type = "UnexpectedError",
                    Error = "An unexpected error occurred",
                    Detail = exception.Message
                }
            };

            context.Response.StatusCode = exception switch
            {
                ResourceNotFoundException => StatusCodes.Status404NotFound,
                ValidationEx => StatusCodes.Status400BadRequest,
                BusinessRuleException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }

        private static string GetUserFriendlyErrorMessage(string errorType) => errorType switch
        {
            "ValidationError" => "The provided data is invalid",
            "ResourceNotFound" => "The requested resource was not found",
            "BusinessRuleViolation" => "A business rule was violated",
            _ => "An error occurred while processing your request"
        };
    }
}
