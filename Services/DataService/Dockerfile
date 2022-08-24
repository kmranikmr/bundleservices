FROM laptevss/dotnet-aspnet AS base
WORKDIR /app
EXPOSE 80
EXPOSE 1433
FROM mcr.microsoft.com/dotnet/core/sdk:2.2  AS build
WORKDIR /src
COPY Services/DataService/DataService.csproj Services/DataService/
COPY Common/Common.Utils/Common.Utils.csproj Common/Common.Utils/
COPY Common/Common.Entities/Common.Entities.csproj Common/Common.Entities/
COPY DataAccess/DataAccess.csproj DataAccess/
RUN dotnet restore Services/DataService/DataService.csproj
COPY . .
WORKDIR /src/Services/DataService
RUN dotnet build DataService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish DataService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DataService.dll"]
  