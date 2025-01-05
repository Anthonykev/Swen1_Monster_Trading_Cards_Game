# Verwenden Sie ein offizielles .NET 6 SDK-Image als Build-Umgebung
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Kopieren Sie die Projektdateien und Wiederherstellen der Abhängigkeiten
COPY *.csproj ./
RUN dotnet restore

# Kopieren Sie den Rest der Anwendung und bauen Sie sie
COPY . ./
RUN dotnet publish -c Release -o out

# Verwenden Sie ein leichtgewichtiges .NET 6 Runtime-Image als Laufzeitumgebung
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# Setzen Sie den Einstiegspunkt für die Anwendung
ENTRYPOINT ["dotnet", "Monster_Traiding_Cards_Game.dll"]
