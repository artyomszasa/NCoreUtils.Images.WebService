using System;
using System.Net;
using System.Runtime.Serialization;

namespace NCoreUtils.Images.WebService;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class RemoteImageCommunicationException : RemoteImageException
{
    public HttpStatusCode HttpStatusCode { get; }

#if !NET8_0_OR_GREATER
    protected RemoteImageCommunicationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => HttpStatusCode = (HttpStatusCode)info.GetInt32(nameof(HttpStatusCode));
#endif

    public RemoteImageCommunicationException(
        string endpoint,
        HttpStatusCode httpStatusCode,
        string description,
        Exception innerException)
        : base(endpoint, RemoteErrorCodes.CommunicationError, description, innerException)
        => HttpStatusCode = httpStatusCode;

    public RemoteImageCommunicationException(
        string endpoint,
        HttpStatusCode httpStatusCode,
        string description)
        : base(endpoint, RemoteErrorCodes.CommunicationError, description)
        => HttpStatusCode = httpStatusCode;

#if !NET8_0_OR_GREATER
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(HttpStatusCode), (int)HttpStatusCode);
    }
#endif
}