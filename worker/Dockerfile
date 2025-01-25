# Utiliser une image officielle .NET Core
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Définir le répertoire de travail
WORKDIR /app

# Copier le code source
COPY . .

# Restaurer les dépendances
RUN dotnet restore

# Construire le projet
RUN dotnet publish -c Release -o /out

# Utiliser une image runtime .NET pour l'exécution
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Définir le répertoire de travail
WORKDIR /app

# Copier les fichiers compilés
COPY --from=build /out .

# Commande pour démarrer l'application
CMD ["dotnet", "Worker.dll"]
