using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NCoreUtils.Images
{
    public class OkNoCacheResult : IActionResult
    {
        public Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = 200;
            response.Headers.Append("Cache-Control", "no-store, no-cache");
            response.ContentType = "text/plain; charset=utf-8";
            return Task.CompletedTask;
        }
    }
}