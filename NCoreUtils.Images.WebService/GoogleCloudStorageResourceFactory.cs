using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using Microsoft.Extensions.Logging;
using NCoreUtils.Images.GoogleCloudStorage;

namespace NCoreUtils.Images
{
    public class GoogleCloudStorageResourceFactory : IResourceFactory
    {
        static readonly HashSet<string> _truthy = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "true",
            "t",
            "on",
            "1"
        };

        static Uri StripQuery(Uri source)
            => new UriBuilder(source) { Query = string.Empty }.Uri;

        readonly ILoggerFactory _loggerFactory;

        readonly ILogger _logger;

        readonly IHttpClientFactory? _httpClientFactory;

        public GoogleCloudStorageResourceFactory(ILoggerFactory loggerFactory, IHttpClientFactory? httpClientFactory = default)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<GoogleCloudStorageResourceFactory>();
            _httpClientFactory = httpClientFactory;
        }

        public IImageDestination CreateDestination(Uri? uri, Func<IImageDestination> next)
        {
            if (null != uri && uri.Scheme == "gs")
            {
                var q = HttpUtility.ParseQueryString(uri.Query);
                var accessToken = q.Get(UriParameters.AccessToken) ?? throw new InvalidOperationException("No access token provided in GCS destination uri.");
                var cacheControl = q.Get(UriParameters.CacheControl);
                var contentType = q.Get(UriParameters.ContentType);
                var rawPublic = q.Get(UriParameters.Public);
                var isPublic = !string.IsNullOrEmpty(rawPublic) && _truthy.Contains(rawPublic);
                return new GoogleCloudStorageDestination(
                    uri: StripQuery(uri),
                    credential: GoogleStorageCredential.ViaAccessToken(accessToken),
                    contentType: contentType,
                    cacheControl: cacheControl,
                    isPublic: isPublic,
                    httpClientFactory: _httpClientFactory,
                    logger: _loggerFactory.CreateLogger<GoogleCloudStorageDestination>()
                );
            }
            return next();
        }

        public IImageSource CreateSource(Uri? uri, Func<IImageSource> next)
        {
            if (null != uri && uri.Scheme == "gs")
            {
                var q = HttpUtility.ParseQueryString(uri.Query);
                var accessToken = q.Get(UriParameters.AccessToken) ?? throw new InvalidOperationException("No access token provided in GCS source uri.");
                return new GoogleCloudStorageSource(
                    uri: StripQuery(uri),
                    credential: GoogleStorageCredential.ViaAccessToken(accessToken),
                    httpClientFactory: _httpClientFactory,
                    logger: _loggerFactory.CreateLogger<GoogleCloudStorageSource>()
                );
            }
            return next();
        }
    }
}