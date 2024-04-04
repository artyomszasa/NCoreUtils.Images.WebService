using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Images.WebService;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class RemoteInternalImageException : InternalImageException, IRemoteImageException
{
    public string EndPoint { get; }

    public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

#if !NET8_0_OR_GREATER
    protected RemoteInternalImageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;
#endif

    public RemoteInternalImageException(string endpoint, string internalCode, string description)
        : base(internalCode, description)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public RemoteInternalImageException(string endpoint, string internalCode, string description, Exception innerException)
        : base(internalCode, description, innerException)
        => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

#if !NET8_0_OR_GREATER
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EndPoint), EndPoint);
    }
#endif
}