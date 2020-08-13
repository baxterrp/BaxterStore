using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BaxterStore.Data.Exceptions
{
    public class DataRepositoryErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        public DataRepositoryErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                await HandleException(httpContext, exception);
            }
        }

        private Task HandleException(HttpContext httpContext, Exception exception)
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

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)code;

            var stringifiedApiException = JsonConvert.SerializeObject(new ApiException(exception.Message));

            return httpContext.Response.WriteAsync(stringifiedApiException);
        }
    }
}
