using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images;

public class ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
            _logger.LogFailedToProcessRequest(exn);
            ImageErrorData data = Generic.ExceptionHelper.GetErrorData(exn);
            response.StatusCode = 400;
            response.ContentType = "application/json; charset=utf-8";
            await ErrorSerialization.SerializeImageErrorDataAsync(response.Body, data, context.RequestAborted).ConfigureAwait(false);
            await response.Body.FlushAsync(context.RequestAborted).ConfigureAwait(false);
        }
    }
}