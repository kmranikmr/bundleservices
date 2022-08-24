FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY GatewayApi/GatewayApi.csproj GatewayApi/
RUN dotnet restore GatewayApi/GatewayApi.csproj
COPY . .
WORKDIR /src/GatewayApi
RUN dotnet build GatewayApi.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish GatewayApi.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "GatewayApi.dll"]
