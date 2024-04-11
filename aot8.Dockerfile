FROM mcr.microsoft.com/dotnet/sdk:8.0.203-bookworm-slim AS build-env
RUN apt update && apt install -y clang zlib1g-dev && apt clean
WORKDIR /app
# RESTORE
COPY ./NuGet.Config ./
COPY ./Directory.Build.props ./
COPY ./NCoreUtils.Images.WebService.Shared/*.csproj ./NCoreUtils.Images.WebService.Shared/
COPY ./NCoreUtils.Images.WebService.Core/*.csproj ./NCoreUtils.Images.WebService.Core/
COPY ./NCoreUtils.Images.WebService/*.csproj ./NCoreUtils.Images.WebService/
RUN sed -i 's/net8.0;net7.0;net6.0;netstandard2.1/net8.0/' ./NCoreUtils.Images.WebService.Shared/NCoreUtils.Images.WebService.Shared.csproj && \
    sed -i 's/net8.0;net7.0;net6.0/net8.0/' ./NCoreUtils.Images.WebService.Core/NCoreUtils.Images.WebService.Core.csproj
RUN dotnet restore ./NCoreUtils.Images.WebService/NCoreUtils.Images.WebService.csproj -r linux-x64 -v n -p EnableAzureBlobStorage=false -p EnableGoogleFluentdLogging=true
# PUBLISH
COPY ./NCoreUtils.Images.WebService.Shared/*.cs ./NCoreUtils.Images.WebService.Shared/
COPY ./NCoreUtils.Images.WebService.Core/*.cs ./NCoreUtils.Images.WebService.Core/
COPY ./NCoreUtils.Images.WebService/*.cs ./NCoreUtils.Images.WebService/
COPY ./NCoreUtils.Images.WebService/*.xml ./NCoreUtils.Images.WebService/
RUN sed -i '/Azure/d' ./NCoreUtils.Images.WebService/NCoreUtils.Images.WebService.trim.xml
RUN dotnet publish ./NCoreUtils.Images.WebService/*.csproj -r linux-x64 -c Release --self-contained -p PublishAot=true -p EnableAzureBlobStorage=false -p EnableGoogleFluentdLogging=true -o /app/out

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.3-bookworm-slim
WORKDIR /app
ENV DOTNET_ENVIRONMENT=Production \
    ASPNETCORE_ENVIRONMENT=Production
COPY --from=build-env /app/out ./
COPY ./run/appsettings.default.json /app/secrets/appsettings.json
ENTRYPOINT ["./NCoreUtils.Images.WebService"]
