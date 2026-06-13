FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TyreServiceApp/TyreServiceApp.sln TyreServiceApp/
COPY TyreServiceApp/TyreServiceApp/TyreServiceApp.csproj TyreServiceApp/TyreServiceApp/
RUN dotnet restore TyreServiceApp/TyreServiceApp.sln

COPY TyreServiceApp/ TyreServiceApp/
WORKDIR /src/TyreServiceApp/TyreServiceApp
RUN dotnet publish TyreServiceApp.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TyreServiceApp.dll"]
