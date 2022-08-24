FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY DataShare/DataShareService.csproj DataShare/
COPY Common/Common.Utils/Common.Utils.csproj Common/Common.Utils/
COPY Common/Common.Entities/Common.Entities.csproj Common/Common.Entities/
RUN dotnet restore DataShare/DataShareService.csproj
COPY . .
WORKDIR /src/DataShare
RUN dotnet build DataShareService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish DataShareService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DataShareService.dll"]
