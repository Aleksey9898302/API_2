using System.Text.Json;

namespace Gamification.API.Middleware
{
    /// <summary>
    /// Middleware логирования запросов: метод, путь, код ответа и время выполнения.
    /// Показывает, что middleware оборачивает ВСЕ запросы конвейера.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var start = DateTime.UtcNow;

            // Код ДО вызова _next — выполняется на входе запроса
            _logger.LogInformation("→ {Method} {Path} начат", context.Request.Method, context.Request.Path);

            await _next(context); // передаём запрос дальше по конвейеру

            // Код ПОСЛЕ _next — выполняется на выходе, когда ответ уже сформирован
            var elapsed = DateTime.UtcNow - start;
            _logger.LogInformation("← {Method} {Path} → {StatusCode} за {ElapsedMs} мс",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Удобный метод регистрации: app.UseRequestLogging();
    /// </summary>
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
