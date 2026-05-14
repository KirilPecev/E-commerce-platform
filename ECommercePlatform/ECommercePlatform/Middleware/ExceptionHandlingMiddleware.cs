using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlingMiddleware> logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Ensure a correlation id is present for the request and response
            const string correlationHeader = "X-Correlation-Id";

            string correlationId = context.Request.Headers.ContainsKey(correlationHeader)
                ? context.Request.Headers[correlationHeader].ToString()
                : Guid.NewGuid().ToString();

            if (!context.Response.Headers.ContainsKey(correlationHeader))
            {
                context.Response.Headers[correlationHeader] = correlationId;
            }

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, correlationId);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
        {
            var traceId = context.TraceIdentifier;
            var path = context.Request.Path;

            var (statusCode, title) = MapExceptionToStatusCode(exception);

            ProblemDetails problem;

            if (statusCode == HttpStatusCode.BadRequest && IsValidationException(exception, out var errors))
            {
                var validationProblem = new ValidationProblemDetails(errors ?? new Dictionary<string, string[]>())
                {
                    Type = "about:blank",
                    Title = title,
                    Status = (int)statusCode,
                    Detail = exception.Message,
                    Instance = path
                };

                problem = validationProblem;
            }
            else
            {
                problem = new ProblemDetails
                {
                    Type = "about:blank",
                    Title = title,
                    Status = (int)statusCode,
                    Detail = exception.Message,
                    Instance = path
                };
            }

            problem.Extensions["traceId"] = traceId;
            problem.Extensions["correlationId"] = correlationId;

            logger.LogError(exception, "Unhandled exception (status {Status}) on request {Path} (traceId: {TraceId}, correlationId: {CorrelationId})", (int)statusCode, path, traceId, correlationId);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            return context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }

        private static (HttpStatusCode statusCode, string title) MapExceptionToStatusCode(Exception ex)
        {
            // Common framework exceptions
            if (ex is KeyNotFoundException || ex.GetType().Name == "NotFoundException")
                return (HttpStatusCode.NotFound, "Resource not found");

            if (ex is UnauthorizedAccessException)
                return (HttpStatusCode.Unauthorized, "Unauthorized");

            if (ex is ArgumentException || ex is ArgumentNullException)
                return (HttpStatusCode.BadRequest, "Bad request");

            if (ex is InvalidOperationException || ex is InvalidOperationException)
                return (HttpStatusCode.BadRequest, "Bad request");

            if (ex is ValidationException || ex.GetType().Name == "ValidationException")
                return (HttpStatusCode.BadRequest, "Validation error");

            // Identity-specific exceptions
            if (ex.GetType().Name == "IdentityException")
                return (HttpStatusCode.BadRequest, "Identity error");

            // Domain exceptions: fallback by name matching (e.g. CatalogDomainException, OrderDomainException)
            if (ex.GetType().Name.EndsWith("DomainException"))
                return (HttpStatusCode.BadRequest, "Domain error");

            // Default to 500
            return (HttpStatusCode.InternalServerError, "An unexpected error occurred");
        }

        private static bool IsValidationException(Exception ex, out Dictionary<string, string[]> errors)
        {
            errors = null;

            // System.ComponentModel.DataAnnotations.ValidationException
            if (ex is ValidationException)
            {
                errors = new Dictionary<string, string[]>
                {
                    [""] = new[] { ex.Message }
                };

                return true;
            }

            // FluentValidation.ValidationException (best-effort via reflection)
            if (ex.GetType().Name == "ValidationException")
            {
                // Try to read an Errors property
                var errorsProperty = ex.GetType().GetProperty("Errors", BindingFlags.Instance | BindingFlags.Public);
                if (errorsProperty != null)
                {
                    var raw = errorsProperty.GetValue(ex) as System.Collections.IEnumerable;
                    if (raw != null)
                    {
                        var dict = new Dictionary<string, List<string>>();

                        foreach (var item in raw)
                        {
                            var propNameProp = item.GetType().GetProperty("PropertyName");
                            var errorMessageProp = item.GetType().GetProperty("ErrorMessage");

                            var propName = propNameProp?.GetValue(item)?.ToString() ?? "";
                            var errorMessage = errorMessageProp?.GetValue(item)?.ToString() ?? "";

                            if (!dict.ContainsKey(propName)) dict[propName] = new List<string>();
                            dict[propName].Add(errorMessage);
                        }

                        errors = dict.ToDictionary(k => k.Key, v => v.Value.ToArray());
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
