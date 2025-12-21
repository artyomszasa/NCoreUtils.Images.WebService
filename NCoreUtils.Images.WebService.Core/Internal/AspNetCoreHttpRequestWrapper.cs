using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NCoreUtils.Images.Internal;

public sealed class AspNetCoreHttpRequestWrapper(HttpRequest request) : Generic.IHttpRequest
{
    private HttpRequest Request { get; } = request ?? throw new ArgumentNullException(nameof(request));

    public string? ContentType => Request.ContentType;

    public Stream Body => Request.Body;

    public bool TryGetQueryParameter(string key, out StringValues values)
        => Request.Query.TryGetValue(key, out values);
}