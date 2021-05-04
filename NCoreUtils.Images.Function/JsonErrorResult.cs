using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images
{
    public class JsonErrorResult : IActionResult
    {
        private static ImageErrorData GetErrorData(ImageException exn)
            => exn switch
            {
                UnsupportedResizeModeException e => new UnsupportedResizeModeData(e.ErrorCode, e.Message, e.ResizeMode, e.Width, e.Height),
                UnsupportedImageTypeException e => new UnsupportedImageTypeData(e.ErrorCode, e.Message, e.ImageType),
                InternalImageException e => new InternalImageErrorData(e.ErrorCode, e.Message, e.InternalCode),
                ImageException e => new ImageErrorData(e.ErrorCode, e.Message)
            };

        private readonly Exception _exception;

        public JsonErrorResult(Exception exception)
            => _exception = exception ?? throw new ArgumentNullException(nameof(exception));

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            ImageErrorData data = _exception is ImageException e ? GetErrorData(e) : new ImageErrorData(ErrorCodes.GenericError, _exception.Message);
            response.StatusCode = 400;
            response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(response.Body, data, ErrorSerialization.Options, context.HttpContext.RequestAborted).ConfigureAwait(false);
            await response.Body.FlushAsync(context.HttpContext.RequestAborted).ConfigureAwait(false);
        }
    }
}