using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Presentation.Attributes
{
    public class RedisCacheAttribute : ActionFilterAttribute
    {
        private readonly int _durationInMins;

        public RedisCacheAttribute(int durationInMins = 5)
        {
            _durationInMins = durationInMins;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next
        )
        {
            //Get CacheService from DI Container
            var cacheService =
                context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
            var logger = context
                .HttpContext.RequestServices.GetRequiredService<
                    ILoggerFactory
                >()
                .CreateLogger<RedisCacheAttribute>();

            //Create CacheKey Based On RequestPath & QueryParams
            var cacheKey = CreateCacheKey(context.HttpContext.Request);

            //The cache is an optimisation, not a dependency. If Redis is unreachable the
            //endpoint must still serve from the database instead of failing the request.
            string? cacheValue = null;
            try
            {
                cacheValue = await cacheService.GetAsync(cacheKey);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cache read failed for {CacheKey}; serving uncached", cacheKey);
            }

            //If Exists,Return Cached Data and skip executing endpoint
            if (cacheValue is not null)
            {
                context.Result = new ContentResult()
                {
                    Content = cacheValue,
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK,
                };

                return;
            }
            //If Not exists ,Execute endpoint and store result in Cache if response from endpoint is 200 Ok

            var ExecutedContext = await next.Invoke();

            if (ExecutedContext.Result is OkObjectResult result)
            {
                try
                {
                    await cacheService.SetAsync(
                        cacheKey,
                        result.Value!,
                        TimeSpan.FromMinutes(_durationInMins)
                    );
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Cache write failed for {CacheKey}", cacheKey);
                }
            }
        }

        // api/Products
        // api/Products?brandId=2
        // api/Products?typeId=1
        // api/Products?brandId=2&typeId=1
        // api/Products?typeId=1 &brandId=2
        private string CreateCacheKey(HttpRequest request)
        {
            StringBuilder key = new StringBuilder();

            key.Append(request.Path); // api/Products|brandId-2|typeId-1

            foreach (var item in request.Query.OrderBy(X => X.Key))
            {
                key.Append($"|{item.Key}-{item.Value}");
            }

            return key.ToString();
        }
    }
}
