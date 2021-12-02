FROM mcr.microsoft.com/dotnet/sdk:6.0.100-bullseye-slim AS installer-env

COPY ./NuGet.Config ./
COPY ./Directory.Build.props ./
COPY ./NCoreUtils.Images.WebService.Shared/*.csproj /src/NCoreUtils.Images.WebService.Shared/
COPY ./NCoreUtils.Images.WebService.Core/*.csproj /src/NCoreUtils.Images.WebService.Core/
COPY ./NCoreUtils.Images.WebService.ResourceFactory.AzureBlobStorage/*.csproj /src/NCoreUtils.Images.WebService.ResourceFactory.AzureBlobStorage/
COPY ./NCoreUtils.Images.Function/*.csproj /src/dotnet-function-app/
WORKDIR /src/dotnet-function-app
RUN dotnet restore -r linux-x64 *.csproj -v n
COPY ./NCoreUtils.Images.WebService.Shared/*.cs /src/NCoreUtils.Images.WebService.Shared/
COPY ./NCoreUtils.Images.WebService.Core/*.cs /src/NCoreUtils.Images.WebService.Core/
COPY ./NCoreUtils.Images.WebService.ResourceFactory.AzureBlobStorage/*.cs /src/NCoreUtils.Images.WebService.ResourceFactory.AzureBlobStorage/
COPY ./NCoreUtils.Images.Function/*.cs /src/dotnet-function-app/
COPY ./NCoreUtils.Images.Function/host.json /src/dotnet-function-app/
RUN mkdir -p /home/site/wwwroot && \
    dotnet publish -r linux-x64 --no-self-contained *.csproj --no-restore --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet:4.0.1.16816
RUN apt update && apt install -y ghostscript && apt clean
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
COPY ./NCoreUtils.Images.Function/appsettings.json /home/site/wwwroot/secrets/appsettings.json
# RUN ln -s /home/site/wwwroot/bin/Magick.NET-Q16-x64.dll /home/site/wwwroot/bin/libMagick.Native-Q16-x64.dll.so