using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Images.WebService
{
    [Serializable]
    public class RemoteInternalImageException : InternalImageException, IRemoteImageException
    {
        public string EndPoint { get; }

        public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

        protected RemoteInternalImageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;

        public RemoteInternalImageException(string endpoint, string internalCode, string description)
            : base(internalCode, description)
            => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        public RemoteInternalImageException(string endpoint, string internalCode, string description, Exception innerException)
            : base(internalCode, description, innerException)
            => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(EndPoint), EndPoint);
        }
    }
}