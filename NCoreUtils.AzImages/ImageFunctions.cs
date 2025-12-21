using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NCoreUtils.Images;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.AzImages;

internal sealed class FakeRequest(string body, IReadOnlyDictionary<string, StringValues> query) : Images.Generic.IHttpRequest
{
    public string? ContentType => "application/json";

    public Stream Body { get; } = new MemoryStream(Convert.FromBase64String(body), writable: false);

    public bool TryGetQueryParameter(string key, out StringValues values)
        => query.TryGetValue(key, out values);
}

public sealed class SerializedRequest(string body, string query)
{
    public string Body { get; } = body;

    public string Query { get; } = query;
}

[JsonSerializable(typeof(SerializedRequest))]
internal partial class SerializedRequestSerializerContext : JsonSerializerContext { }

public class ImageFunctions(ILogger<ImageFunctions> logger, IResourceFactory resourceFactory, IImageResizer imageResizer, IImageAnalyzer imageAnalyzer)
{
    private static long? GetLength(Stream source)
    {
        try
        {
            return source.Length;
        }
        catch
        {
            return default;
        }
    }

    private static async Task<HttpResponseData> Error(HttpRequestData request, Exception exn, CancellationToken cancellationToken)
    {
        var data = Images.Generic.ExceptionHelper.GetErrorData(exn);
        var response = request.CreateResponse(System.Net.HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await ErrorSerialization.SerializeImageErrorDataAsync(response.Body, data, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private static HttpResponseData Ok(HttpRequestData request)
        => request.CreateResponse(System.Net.HttpStatusCode.OK);

    private ILogger<ImageFunctions> Logger { get; } = logger;

    private IResourceFactory ResourceFactory { get; } = resourceFactory;

    private IImageResizer ImageResizer { get; } = imageResizer;

    private IImageAnalyzer ImageAnalyzer { get; } = imageAnalyzer;

    [Function("Resize")]
    public async Task<HttpResponseData> RunResize([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "img")] HttpRequestData request)
    {
        try
        {
            Images.Generic.Cleanup.PerformCleanup();
            var contentType = request.Headers.TryGetValues("Content-Type", out var values) ? values.FirstOrDefault() : default;
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("Starting resize request [Length = {ContentLength}, Type = {ContentType}].", GetLength(request.Body), contentType);
            }
            await CoreFunctions
                .InvokeResize(request, ResourceFactory, ImageResizer, Logger, request.FunctionContext.CancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exn)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(exn, "Failed to process request: {Exception}.", exn);
            }
            return await Error(request, exn, request.FunctionContext.CancellationToken);
        }
        return Ok(request);
    }

    [Function("Analyze")]
    public async Task<HttpResponseData> RunAnalyze([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "img/" + Routes.Info)] HttpRequestData request)
    {
        try
        {
            Images.Generic.Cleanup.PerformCleanup();
            var contentType = request.Headers.TryGetValues("Content-Type", out var values) ? values.FirstOrDefault() : default;
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("Starting resize request [Length = {ContentLength}, Type = {ContentType}].", GetLength(request.Body), contentType);
            }
            return await CoreFunctions.InvokeAnalyze(request, ResourceFactory, ImageAnalyzer, Logger, request.FunctionContext.CancellationToken);
        }
        catch (Exception exn)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(exn, "Failed to process request: {Exception}.", exn);
            }
            return await Error(request, exn, request.FunctionContext.CancellationToken);
        }
    }

    [Function("Capabilities")]
    public static HttpResponseData RunCapabilities([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "img/" + Routes.Capabilities)] HttpRequestData request)
    {
        var response = request.CreateResponse(System.Net.HttpStatusCode.OK);
        CoreFunctions.InvokeCapabilities(response, request.FunctionContext.CancellationToken);
        return response;
    }

    [Function(nameof(RunScheduledResize))]
    public async Task RunScheduledResize([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<string>() ?? throw new InvalidOperationException("Input is null.");
        var logger = context.CreateReplaySafeLogger<ImageFunctions>();
        logger.LogInformation("Starting scheduled resize request [Length = {ContentLength}].", input.Length);
        await context.CallActivityAsync(nameof(DoRunScheduledResize), input);
    }

    [Function(nameof(DoRunScheduledResize))]
    public async Task DoRunScheduledResize([ActivityTrigger] string serializedRequest, FunctionContext functionContext)
    {
        var requestData = JsonSerializer.Deserialize(serializedRequest, SerializedRequestSerializerContext.Default.SerializedRequest)
            ?? throw new InvalidOperationException("Could not deserialize request.");
        var query = QueryHelpers.ParseQuery(requestData.Query);
        var request = new FakeRequest(requestData.Body, query);
        await Images.Generic.CoreFunctions
            .InvokeResize(request, ResourceFactory, ImageResizer, Logger, functionContext.CancellationToken)
            .ConfigureAwait(false);
    }

    [Function("ScheduleResize")]
    public async Task<HttpResponseData> RunScheduleResize(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "img/schedule")] HttpRequestData request,
        [DurableClient] DurableTaskClient client)
    {
        var query = request.Url.Query;
        string body;
        {
            var buffer = new byte[4 * 1024];
            await using var bufferStream = new MemoryStream(buffer, writable: true);
            await request.Body.CopyToAsync(bufferStream, request.FunctionContext.CancellationToken).ConfigureAwait(false);
            body = Convert.ToBase64String(buffer.AsSpan()[..(int)bufferStream.Position]);
        }
        // var retryOpts = TaskRetryOptions.FromRetryHandler(context =>
        // {
        //     return context.LastAttemptNumber < 3;
        // });
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            orchestratorName: nameof(RunScheduledResize),
            input: JsonSerializer.Serialize(new SerializedRequest(body, query), SerializedRequestSerializerContext.Default.SerializedRequest),
            // options: new StartOrchestrationOptions(retryOpts)
            cancellation: request.FunctionContext.CancellationToken
        );

        request.FunctionContext.GetLogger<ImageFunctions>()
            .LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(request, instanceId);
    }
}