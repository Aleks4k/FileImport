using FileImport.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace FileImport.Api.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;
        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            _logger = logger;
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
                { typeof(InvalidFilePathException), HandleInvalidFilePathException },
                { typeof(ArgumentException), HandleArgumentException }
            };
        }
        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Greška prilikom izvršavanja akcije na putanji {Path}", context.HttpContext.Request.Path);
            HandleException(context);
        }
        private void HandleException(ExceptionContext context)
        {
            var type = context.Exception.GetType();
            if (_exceptionHandlers.TryGetValue(type, out var handler))
            {
                handler.Invoke(context);
                return;
            }
            HandleUnknownException(context);
        }
        private void HandleUnknownException(ExceptionContext context)
        {
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
        }
        private void HandleUnauthorizedAccessException(ExceptionContext context)
        {
            var exception = (UnauthorizedAccessException)context.Exception;
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status405MethodNotAllowed,
                Title = exception.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            };
            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status405MethodNotAllowed
            };
            context.ExceptionHandled = true;
        }
        private void HandleInvalidFilePathException(ExceptionContext context)
        {
            var exception = (InvalidFilePathException)context.Exception;
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = exception.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            };
            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            context.ExceptionHandled = true;
        }
        private void HandleArgumentException(ExceptionContext context)
        {
            var exception = (ArgumentException)context.Exception;
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = exception.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            };
            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            context.ExceptionHandled = true;
        }
    }
}