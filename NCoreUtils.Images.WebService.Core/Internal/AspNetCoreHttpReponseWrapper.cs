using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.Images.Internal;

public sealed class AspNetCoreHttpResponseWrapper(HttpResponse response) : Generic.IHttpResponse
{
    private HttpResponse Response { get; } = response ?? throw new ArgumentNullException(nameof(response));

    public string ContentType { set => Response.ContentType = value; }

    public long? ContentLength { set => Response.ContentLength = value; }

    public Stream Body => Response.Body;
}