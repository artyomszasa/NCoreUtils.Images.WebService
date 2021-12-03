using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images
{
    public class ErrorMiddleware
    {
        static ImageErrorData GetErrorData(ImageException exn)
            => exn switch
            {
                UnsupportedResizeModeException e => new UnsupportedResizeModeData(e.ErrorCode, e.Message, e.ResizeMode, e.Width, e.Height),
                UnsupportedImageTypeException e => new UnsupportedImageTypeData(e.ErrorCode, e.Message, e.ImageType),
                InternalImageException e => new InternalImageErrorData(e.ErrorCode, e.Message, e.InternalCode),
                ImageException e => new ImageErrorData(e.ErrorCode, e.Message)
            };

        readonly RequestDelegate _next;

        readonly ILogger _logger;

        public ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception exn)
            {
                var response = context.Response;
                if (response.HasStarted)
                {
                    throw;
                }
                _logger.LogWarning(exn, "Failes to process request.");
                ImageErrorData data = exn is ImageException e ? GetErrorData(e) : new ImageErrorData(ErrorCodes.GenericError, exn.Message);
                response.StatusCode = 400;
                response.ContentType = "application/json; charset=utf-8";
                await ErrorSerialization.SerializeImageErrorDataAsync(response.Body, data, context.RequestAborted).ConfigureAwait(false);
                await response.Body.FlushAsync(context.RequestAborted).ConfigureAwait(false);
            }
        }
    }
}