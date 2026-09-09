using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ResortBooking.API.Infrastructure
{
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger):IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,Exception exception,CancellationToken cancellationToken)
        {
            logger.LogError(exception,"Unhandled exception while processing {Method} {Path}",httpContext.Request.Method,httpContext.Request.Path);
            var statusCode = exception switch
            {
                ArgumentException =>StatusCodes.Status400BadRequest,

                InvalidOperationException =>StatusCodes.Status400BadRequest,

                _ =>StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,

                Title = statusCode ==StatusCodes.Status500InternalServerError? "An unexpected error occurred." : "The request could not be completed.",

                Detail = statusCode ==StatusCodes.Status500InternalServerError ? "An unexpected error occurred while processing the request." : exception.Message,

                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
