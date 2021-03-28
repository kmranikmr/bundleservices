FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY Services/FileUpload/FileUploadAPI/FileUploadService/FileUploadService.csproj Services/FileUpload/FileUploadAPI/FileUploadService/
RUN dotnet restore Services/FileUpload/FileUploadAPI/FileUploadService/FileUploadService.csproj
COPY . .
WORKDIR /src/Services/FileUpload/FileUploadAPI/FileUploadService
RUN dotnet build FileUploadService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish FileUploadService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FileUploadService.dll"]
