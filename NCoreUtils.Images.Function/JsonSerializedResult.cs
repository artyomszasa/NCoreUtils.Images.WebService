using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NCoreUtils.Images
{
    public class JsonSerializedResult<T> : IActionResult
    {
        private readonly T _data;

        private readonly JsonTypeInfo<T> _typeInfo;

        public JsonSerializedResult(T data, JsonTypeInfo<T> typeInfo)
        {
            _data = data;
            _typeInfo = typeInfo;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(response.Body, _data, _typeInfo, context.HttpContext.RequestAborted).ConfigureAwait(false);
            await response.Body.FlushAsync(context.HttpContext.RequestAborted).ConfigureAwait(false);
        }
    }
}