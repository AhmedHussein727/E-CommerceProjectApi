using ECommerce.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.CustomMiddlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlerMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next.Invoke(httpContext);

                if (
                    httpContext.Response.StatusCode == StatusCodes.Status404NotFound
                    && !httpContext.Response.HasStarted
                )
                {
                    var proplem = new ProblemDetails()
                    {
                        Title = "Error while processing HTTP Request -End point Not found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = $"Endpoint {httpContext.Request.Path} not found",
                        Instance = httpContext.Request.Path,
                    };

                    await httpContext.Response.WriteAsJsonAsync(proplem);
                }
            }
            catch (Exception ex)
            {
                //Logging
                _logger.LogError(ex, "Something went wrong");
                //Return custom Error Response
                //  httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var problem = new ProblemDetails()
                {
                    Title = "An unexpected error occured",
                    Detail = ex.Message,
                    Instance = httpContext.Request.Path,
                    Status = ex switch
                    {
                        NotFoundException => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status500InternalServerError,
                    },
                };

                httpContext.Response.StatusCode = problem.Status.Value;

                await httpContext.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
