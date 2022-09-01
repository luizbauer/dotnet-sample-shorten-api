namespace Shorten.API
{
    public class AnalyticsMiddleware : IMiddleware
    {
        private readonly ILogger<AnalyticsMiddleware> _logger;

        public AnalyticsMiddleware(ILogger<AnalyticsMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogInformation("Sent some Analytics Data");
            await next(context);
        }
    }
}
