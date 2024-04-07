using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images
{
    public class ImageFunctions
    {
        private IResourceFactory ResourceFactory { get; }

        private IImageResizer ImageResizer { get; }

        private IImageAnalyzer ImageAnalyzer { get; }

        public ImageFunctions(IResourceFactory resourceFactory, IImageResizer imageResizer, IImageAnalyzer imageAnalyzer)
        {
            ResourceFactory = resourceFactory;
            ImageResizer = imageResizer;
            ImageAnalyzer = imageAnalyzer;
        }

        [FunctionName("Resize")]
        public async Task<IActionResult> RunResize(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "img")] HttpRequest request,
            ILogger log,
            CancellationToken hostCancellationToken)
        {
            Cleanup.PerformCleanup();
            // cancellation
            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken, request.HttpContext.RequestAborted);
            try
            {
                await CoreFunctions.InvokeResize(request, ResourceFactory, ImageResizer, cancellationSource.Token);
            }
            catch (Exception exn)
            {
                log.LogFailedToProcessRequest(exn);
                return new JsonErrorResult(exn);
            }
            return new OkNoCacheResult();
        }

        [FunctionName("Analyze")]
        public async Task<IActionResult> RunAnalyze(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "img/" + Routes.Info)] HttpRequest request,
            ILogger log,
            CancellationToken hostCancellationToken)
        {
            Cleanup.PerformCleanup();
            // cancellation
            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken, request.HttpContext.RequestAborted);
            try
            {
                var res = await CoreFunctions.InvokeAnalyze(request, ResourceFactory, ImageAnalyzer, cancellationSource.Token);
                return new JsonSerializedResult<ImageInfo>(res, ImageInfoJsonContext.Default.ImageInfo);
            }
            catch (Exception exn)
            {
                log.LogFailedToProcessRequest(exn);
                return new JsonErrorResult(exn);
            }
        }

        [FunctionName("Capabilities")]
        public static IActionResult RunCapabilities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "img/" + Routes.Capabilities)] HttpRequest request,
            ILogger log)
            => CapabilitiesResult.Singleton;
    }
}
