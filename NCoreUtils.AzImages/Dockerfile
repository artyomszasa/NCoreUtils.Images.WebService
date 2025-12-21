FROM mcr.microsoft.com/dotnet/sdk:10.0 AS installer-env

# COPY . /src/dotnet-function-app
# RUN cd /src/dotnet-function-app && \
# mkdir -p /home/site/wwwroot && \
# dotnet publish *.csproj --output /home/site/wwwroot

WORKDIR /src
COPY ./NuGet.Config /src/
COPY ./Directory.Build.props /src/
COPY ./NCoreUtils.Images.WebService.Shared /src/NCoreUtils.Images.WebService.Shared/
COPY ./NCoreUtils.Images.WebService.Core.Generic /src/NCoreUtils.Images.WebService.Core.Generic/
COPY ./NCoreUtils.AzImages /src/dotnet-function-app/
RUN mkdir -p /home/site/wwwroot && \
    dotnet publish -c Release -r linux-x64 --no-self-contained ./dotnet-function-app/NCoreUtils.AzImages.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0
RUN apt update && apt install -y ghostscript && apt clean
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]