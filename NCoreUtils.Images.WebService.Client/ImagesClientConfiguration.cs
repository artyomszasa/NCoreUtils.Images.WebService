using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images
{
    public class ImagesClientConfiguration
    {
        public const string DefaultHttpClient = "NCoreUtils.Images";

        public string EndPoint { get; set; } = string.Empty;

        public bool AllowInlineData { get; set; } = false;

        public bool CacheCapabilities { get; set; } = true;

        public string HttpClient { get; set; } = DefaultHttpClient;

        internal ImagesClientConfiguration<T> AsTyped<T>()
            where T : ImagesClient
            => new ImagesClientConfiguration<T>
            {
                EndPoint = EndPoint,
                AllowInlineData = AllowInlineData,
                CacheCapabilities = CacheCapabilities,
                HttpClient = HttpClient
            };
    }

    public class ImagesClientConfiguration<T> : ImagesClientConfiguration
        where T : ImagesClient
    { }
}