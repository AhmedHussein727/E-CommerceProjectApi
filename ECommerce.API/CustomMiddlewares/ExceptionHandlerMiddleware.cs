using ECommerce.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.CustomMiddlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlerMiddleware> logger,
            IHostEnvironment env
        )
        {
            _next = next;
            _logger = logger;
            _env = env;
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
                _logger.LogError(ex, "Unhandled exception for {Path}", httpContext.Request.Path);

                //If the response is already on the wire we cannot rewrite it - let it bubble up
                if (httpContext.Response.HasStarted)
                    throw;

                var status = ex switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError,
                };

                //NotFoundException messages are safe to surface; anything else may leak
                //internal details (SQL, stack shape, file paths) so it is hidden outside Development.
                var detail =
                    status == StatusCodes.Status404NotFound || _env.IsDevelopment()
                        ? ex.Message
                        : "An unexpected error occurred. Please try again later.";

                var problem = new ProblemDetails()
                {
                    Title = "An unexpected error occured",
                    Detail = detail,
                    Instance = httpContext.Request.Path,
                    Status = status,
                };

                httpContext.Response.StatusCode = problem.Status.Value;

                await httpContext.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
