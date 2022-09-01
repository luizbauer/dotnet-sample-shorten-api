using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;

namespace Shorten.API
{
    public class CacheAttribute : ActionFilterAttribute
    {
        private readonly long _duration;
        private readonly string _argument;

        public CacheAttribute(long duration, string argument)
        {
            _duration = duration;
            _argument = argument;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.TryGetValue(_argument, out var argument) && argument is not null)
            {
                var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CacheAttribute>>();

                var link = await cache.GetStringAsync(argument.ToString());
                logger.LogInformation("Retrieved from cache: {parameter} - {url}", argument, link);

                if (!string.IsNullOrWhiteSpace(link))
                {
                    context.HttpContext.Response.Redirect(link);
                    return;
                }
            }
            
            await base.OnActionExecutionAsync(context, next);
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is RedirectResult redirect)
            {
                var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CacheAttribute>>();
                
                var parameter = context.HttpContext.Request.RouteValues[_argument]?.ToString();
            
                if (parameter is not null)
                {
                    logger.LogInformation("Caching: {parameter} - {url}", parameter, redirect.Url);
                    await cache.SetStringAsync(parameter, redirect.Url, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_duration)
                    });
                }
            }
            await base.OnResultExecutionAsync(context, next);
        }

    }
}
