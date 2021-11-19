using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Images.WebService
{
    [Serializable]
    public class RemoteImageException : ImageException, IRemoteImageException
    {
        public string EndPoint { get; }

        public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

        protected RemoteImageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => EndPoint = info.GetString(nameof(EndPoint)) ?? string.Empty;

        public RemoteImageException(string endpoint, string errorCode, string description)
            : base(errorCode, description)
            => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        public RemoteImageException(string endpoint, string errorCode, string description, Exception innerException)
            : base(errorCode, description, innerException)
            => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(EndPoint), EndPoint);
        }
    }
}