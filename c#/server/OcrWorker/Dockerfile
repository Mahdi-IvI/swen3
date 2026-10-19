FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY . /src
RUN dotnet publish /src/OcrWorker/OcrWorker.csproj -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "OcrWorker.dll"]
