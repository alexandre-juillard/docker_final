# Utiliser une image Node.js
FROM node:18-slim

# Définir le répertoire de travail
WORKDIR /app

# Copier les fichiers package.json et package-lock.json
COPY package*.json ./

# Installer les dépendances
RUN npm install --only=production

# Copier le reste de l'application
COPY . .

# Exposer le port sur lequel l'application fonctionne
EXPOSE 3000

# Commande pour démarrer l'application
CMD ["npm", "start"]
