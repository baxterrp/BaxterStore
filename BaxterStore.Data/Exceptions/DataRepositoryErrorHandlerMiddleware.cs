using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace BaxterStore.Data.Exceptions
{
    public class DataRepositoryErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DataRepositoryErrorHandlerMiddleware> _logger;

        public DataRepositoryErrorHandlerMiddleware(RequestDelegate next, ILogger<DataRepositoryErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                await WriteHttpContextResponse(httpContext, exception);
            }
        }

        private async Task WriteHttpContextResponse(HttpContext httpContext, Exception exception)
        { 
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            httpContext.Response.StatusCode = GetHttpStatusCode(exception);

            var stringifiedApiException = JsonConvert.SerializeObject(new ApiExceptionContainer(exception.Message));

            _logger.LogTrace(exception.Message);

            await httpContext.Response.WriteAsync(stringifiedApiException);
        }

        private int GetHttpStatusCode(Exception exception)
        {
            HttpStatusCode code;

            switch (exception)
            {
                case ArgumentNullException _:
                case ArgumentException _:
                    code = HttpStatusCode.BadRequest;
                    break;
                case DuplicateResourceException _:
                case ResourceNotFoundException _:
                    code = HttpStatusCode.Conflict;
                    break;
                case InvalidLoginAttemptException _:
                    code = HttpStatusCode.Unauthorized;
                    break;
                default:
                    code = HttpStatusCode.InternalServerError;
                    break;
            }

            return (int)code;
        }
    }
}
