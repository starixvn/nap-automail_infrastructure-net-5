FROM hub.nexdev.net/base/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder
WORKDIR /src

COPY ./Fanex.MailStation.Service/Fanex.MailDelivery.Core ./Fanex.MailDelivery.Core
COPY ./Fanex.MailStation.Service/Fanex.MailDelivery.Services ./Fanex.MailDelivery.Services
COPY ./Fanex.MailStation.Service/Fanex.MailStation.Service/Fanex.MailStation.Service.csproj ./Fanex.MailStation.Service/Fanex.MailStation.Service.csproj

RUN ls /src -l
WORKDIR /src/Fanex.MailStation.Service
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages
RUN dotnet restore -v n

COPY ./Fanex.MailStation.Service/Fanex.MailStation.Service .

RUN --mount=type=cache,target=/root/.nuget/packages
RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:5.0-nap-mail-core


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
