FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY . /src
RUN dotnet publish /src/Api/Api.csproj -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Api.dll"]
