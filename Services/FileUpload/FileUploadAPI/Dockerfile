FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 1433

FROM microsoft/dotnet:2.2-sdk AS build
COPY ./*.sln ./
COPY FileUploadService/ /FileUploadService/
COPY ./FileUploadService/FileUploadService.csproj ./FileUploadService/FileUploadService.csproj
COPY ./Common/Common.csproj ./Common/Common.csproj

RUN dotnet restore 
COPY . .

WORKDIR /Common
RUN dotnet build -c Release -o /app
WORKDIR /FileUploadService
RUN dotnet build -c Release -o /app


FROM build AS publish
RUN dotnet publish  -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FileUploadService.dll"]
