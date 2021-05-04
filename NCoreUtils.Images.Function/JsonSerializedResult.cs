using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NCoreUtils.Images
{
    public class JsonSerializedResult<T> : IActionResult
    {
        private readonly T _data;

        private readonly JsonSerializerOptions? _options;

        public JsonSerializedResult(T data, JsonSerializerOptions? options = default)
        {
            _data = data;
            _options = options;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(response.Body, _data, _options, context.HttpContext.RequestAborted).ConfigureAwait(false);
            await response.Body.FlushAsync(context.HttpContext.RequestAborted).ConfigureAwait(false);
        }
    }
}