ARG CI_REGISTRY_IMAGE
FROM $CI_REGISTRY_IMAGE:builder as deps
RUN mkdir -p /src/packages

FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder

WORKDIR /src

COPY ./Fanex.MailStation.Service/Fanex.SimpleMailDelivery ./Fanex.SimpleMailDelivery
COPY ./Fanex.MailStation.Service/Fanex.MailDelivery.Core ./Fanex.MailDelivery.Core
COPY ./Fanex.MailStation.Service/Fanex.MailDelivery.Services ./Fanex.MailDelivery.Services
COPY ./Fanex.MailStation.Service/Fanex.MailStation.Service/Fanex.MailStation.Service.csproj ./Fanex.MailStation.Service/Fanex.MailStation.Service.csproj

WORKDIR /src/Fanex.MailStation.Service
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN dotnet restore -v n -r linux-x64

COPY ./Fanex.MailStation.Service/Fanex.MailStation.Service .

RUN dotnet publish -r linux-x64 --self-contained false -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-nap-mail-core
# FROM rebuild-baseimage/dotnetcore/aspnet:9.0-nap-mail-core
FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-nap-mail-core

# Don’t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser
#

ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Fanex.MailStation.Service.dll"]
