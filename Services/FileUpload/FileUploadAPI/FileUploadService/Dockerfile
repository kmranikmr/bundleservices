FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
#FROM mcr.microsoft.com/dotnet/core/runtime:3.1.4-buster-slim AS base
WORKDIR /app
EXPOSE 80
#RUN apt-get update -y && apt-get install python3.7 -y \
#python3-pip
#RUN python3.7 -m python-pip install pip
#RUN python3-pip install wfdb
#RUN python3-pip install heartpy

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
#RUN apt-get update -y && apt-get install python3.7 -y
#RUN python3.7 -m pip install pip
#RUN pip install wfdb
#RUN pip install pandas
#RUN pip install numpy
#FROM digdag/digdag-python:3.7
#RUN pip install wfdb
#RUN pip install heartpy
#RUN pip install pandas

ENTRYPOINT ["dotnet", "FileUploadService.dll"]
