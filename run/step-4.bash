#!/usr/bin/env bash

# Étape 4 - Worker
# Version de .NET Core utilisée lors des développements : 7.0

cd ../worker || exit

# Installation des dépendances
dotnet restore

# Production d'un exécutable
dotnet publish -c release --self-contained false --no-restore

# Lancement du serveur
dotnet bin/release/net7.0/Worker.dll
