FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-buster AS build-env
# PREFETCH
WORKDIR /
RUN mkdir -p /tmp/prefetch && \
    cd /tmp/prefetch && \
    dotnet new web && \
    dotnet add package Google.Apis --version 1.44.1 --no-restore && \
    dotnet add package Google.Apis.Auth --version 1.44.1 --no-restore && \
    dotnet add package Google.Apis.Core --version 1.44.1 --no-restore && \
    dotnet add package Magick.NET-Q16-x64 --version 7.15.4 --no-restore && \
    dotnet add package Microsoft.NETCore.Platforms --version 3.1.0 --no-restore && \
    dotnet add package Microsoft.NETCore.Targets --version 3.1.0 --no-restore && \
    dotnet add package NETStandard.Library --version 2.0.3 --no-restore && \
    dotnet add package Newtonsoft.Json --version 12.0.3 --no-restore && \
    dotnet add package System.Buffers --version 4.5.0 --no-restore && \
    dotnet add package System.IO.Pipelines --version 4.7.0 --no-restore && \
    dotnet add package System.Memory --version 4.5.3 --no-restore && \
    dotnet add package System.Numerics.Vectors --version 4.5.0 --no-restore && \
    dotnet add package System.Runtime.CompilerServices.Unsafe --version 4.7.0 --no-restore && \
    dotnet add package System.Text.Json --version 4.7.1 --no-restore && \
    dotnet add package System.ValueTuple --version 4.5.0 --no-restore && \
    dotnet restore -r linux-x64 -v n && \
    cd / && \
    rm -rf /tmp/prefetch
WORKDIR /app
# RESTORE
COPY ./NuGet.Config ./
COPY ./NCoreUtils.Images.WebService.Shared/*.csproj ./NCoreUtils.Images.WebService.Shared/
COPY ./NCoreUtils.Images.WebService/*.csproj ./NCoreUtils.Images.WebService/
RUN dotnet restore ./NCoreUtils.Images.WebService/*.csproj -r linux-x64 -v n
# PUBLISH
COPY ./NCoreUtils.Images.WebService.Shared/*.cs ./NCoreUtils.Images.WebService.Shared/
COPY ./NCoreUtils.Images.WebService/*.cs ./NCoreUtils.Images.WebService/
RUN dotnet publish ./NCoreUtils.Images.WebService/*.csproj -r linux-x64 -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1.3-buster-slim
WORKDIR /app
ENV DOTNET_ENVIRONMENT=Production \
    ASPNETCORE_ENVIRONMENT=Production \
    LISTEN=0.0.0.0:80
COPY --from=build-env /app/out ./
COPY ./run/appsettings.default.json /app/secrets/appsettings.json
ENTRYPOINT ["./NCoreUtils.Images.WebService"]
