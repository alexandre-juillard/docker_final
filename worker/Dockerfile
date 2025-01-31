# Etape 1 : Utiliser une image officielle .NET Core
# image pour compiler et publier le projet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Définir le répertoire de travail
# WORKDIR /app
WORKDIR /src

# Copier uniquement les fichiers de projet nécessaires pour restaurer les dépendances
# (cela permet de ne pas restaurer les dépendances à chaque modification du code source)
COPY *.csproj .

# Restaurer les dépendances
RUN dotnet restore

# Copier le code source
COPY . .

# Supprimer les dossiers bin/ et obj/ avant publication pour réinitialiser le projet
RUN rm -rf bin/ obj/

# Construire le projet
# RUN dotnet publish -c Release -o /out
# construire un executable optimisé pour la production
RUN dotnet publish -c Release --self-contained false -o /app/publish

# Étape 2 : Utiliser une image de runtime .NET pour l'exécution
# image + légère car contient uniquement le runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Définir le répertoire de travail
WORKDIR /app

# Créer un utilisateur non-root
RUN useradd -m myuser

# Copier les fichiers compilés
COPY --from=build /app/publish .

# Passer à l'utilisateur non-root
USER myuser

# Commande pour démarrer l'application
ENTRYPOINT ["dotnet", "Worker.dll"]
