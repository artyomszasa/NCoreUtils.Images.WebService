using System.IO;

namespace NCoreUtils.Images.Generic;

public interface IHttpResponse
{
    string ContentType { set; }

    long? ContentLength { set; }

    Stream Body { get; }
}