ARG CI_REGISTRY_IMAGE
FROM $CI_REGISTRY_IMAGE:builder as deps
RUN mkdir -p /src/packages

FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder

WORKDIR /src

COPY ./Fanex.MailDeliver.Service/Fanex.MailDeliver.Utility ./Fanex.MailDeliver.Utility
COPY ./Fanex.MailDeliver.Service/Fanex.MailDeliver.MailClient ./Fanex.MailDeliver.MailClient
COPY ./Fanex.MailDeliver.Service/Fanex.MailDeliver.Core ./Fanex.MailDeliver.Core
COPY ./Fanex.MailDeliver.Service/Fanex.MailDeliver.Service/Fanex.MailDeliver.Service.csproj ./Fanex.MailDeliver.Service/Fanex.MailDeliver.Service.csproj

WORKDIR /src/Fanex.MailDeliver.Service
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN dotnet restore -v n -r linux-x64 

COPY ./Fanex.MailDeliver.Service/Fanex.MailDeliver.Service .

RUN dotnet publish -r linux-x64 --self-contained false -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-nap-mail-core
# FROM rebuild-baseimage/dotnetcore/aspnet:9.0-nap-mail-core
FROM hub.nexdev.net/rebuild-baseimage/dotnetcore/aspnet:9.0-nap-mail-core

ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0 

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Fanex.MailDeliver.Service.dll"]
