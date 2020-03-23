using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Images.WebService
{
    [Serializable]
    public class RemoteUnsupportedImageTypeException : UnsupportedImageTypeException, IRemoteImageException
    {
        public string EndPoint { get; }

        public override string Message => $"{base.Message} [EndPoint = {EndPoint}]";

        protected RemoteUnsupportedImageTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => EndPoint = info.GetString(nameof(EndPoint));

        public RemoteUnsupportedImageTypeException(string endpoint, string imageType, string description)
            : base(imageType, description)
            => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        public RemoteUnsupportedImageTypeException(string endpoint, string imageType, string description, Exception innerException)
            : base(imageType, description, innerException)
            => EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(EndPoint), EndPoint);
        }
    }
}