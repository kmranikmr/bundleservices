FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
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
