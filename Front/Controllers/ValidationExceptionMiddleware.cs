using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Front.Controllers
{
    public class ValidationExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (FluentValidation.ValidationException e)
            {
                var status = HttpStatusCode.BadRequest;
                var resultToSerialize = new {e.Message, e.StackTrace};
                var result = JsonConvert.SerializeObject(resultToSerialize);
                context.Response.StatusCode = (int) status;
                await context.Response.WriteAsync(result);
            }
        }
    }
}