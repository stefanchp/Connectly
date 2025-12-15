# 1. FAZA DE CONSTRUIRE (BUILD STAGE)
# Folosim .NET 9.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiaza si foloseste fisierele esentiale (NuGet.config, connectly.csproj)
# Folosim sintaxa simplificata, deoarece Dockerfile este la nivelul radacina al proiectului.
COPY NuGet.config .
COPY connectly.csproj . 

# Ruleaza restore folosind setarile curate din NuGet.config
RUN dotnet restore connectly.csproj

# 2. Copiaza restul codului sursa
COPY . .

# 3. Publica aplicatia in modul Release
RUN dotnet publish connectly.csproj -c Release -o /app/publish

# 2. FAZA FINALA (RUNTIME STAGE)
# Folosim imaginea de ASP.NET Runtime 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiaza fisierele publicate din faza de build
COPY --from=build /app/publish .

# Seteaza portul intern al containerului la 8080 (pentru a corespunde cu maparea din docker-compose.yml)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Punctul de intrare in aplicatie
ENTRYPOINT ["dotnet", "connectly.dll"]