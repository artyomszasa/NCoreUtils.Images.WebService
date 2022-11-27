using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NCoreUtils.Images;

public class CapabilitiesResult : IActionResult
{
    public static CapabilitiesResult Singleton { get; } = new();

    public Task ExecuteResultAsync(ActionContext context)
        => CoreFunctions.InvokeCapabilities(context.HttpContext.Response, context.HttpContext.RequestAborted);
}