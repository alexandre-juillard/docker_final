# Etape 1:  Utiliser une image Node.js pour le build
# on utilise alpine qui est une image allegée de node
FROM node:18-slim AS build

# Définir le répertoire de travail
WORKDIR /app

# Supprimer le dossier node_modules s'il existe (pour réinitialiser le projet)
RUN rm -rf node_modules

# Copier les fichiers package.json et package-lock.json
# évite de réinstaller les dépendances à chaque modification
COPY package*.json ./

# Installer les dépendances
# pour ne pas installer les dépendances de développement
RUN npm ci --only=production

# Copier le reste de l'application
COPY . .

# Étape 2 : Créer une image finale légère
# copie uniquement les fichiers nécessaires pour l'exécution
FROM node:18-alpine

# Créer un utilisateur non-root
RUN adduser -D myuser

# Définir le répertoire de travail
WORKDIR /app

# Copier les dépendances installées à l'étape build
COPY --from=build /app/node_modules ./node_modules

# Copier le reste de l'application
COPY --from=build /app .

# Passer à l'utilisateur non-root
USER myuser

# Définir l'environnement pour la production
ENV NODE_ENV=production

# Exposer le port sur lequel l'application fonctionne
EXPOSE 3000

# Commande pour démarrer l'application
CMD ["npm", "start"]
