using System.Text;

namespace NCoreUtils.Images.WebService
{
    internal class ErrorDeserializer
    {
        protected static readonly byte[] _keyDescription = Encoding.ASCII.GetBytes(ImageErrorProperties.Description);

        protected string ErrorCode { get; }

        protected string Description { get; set; } = string.Empty;

        public ErrorDeserializer(string errorCode)
            => ErrorCode = errorCode;
    }
}