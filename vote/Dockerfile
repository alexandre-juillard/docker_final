# Etape 1 : Utiliser une image Python 3.11
# cette étape installe les dépendances 
# image slim + petite pour image + légère
FROM python:3.11-slim AS build

# Définir le répertoire de travail
WORKDIR /app

# Copier les dépendances pour les installer
# évite de réinstaller les dépendances à chaque modification
COPY requirements.txt .

# Option 1:Installer les dépendances
# RUN pip install --no-cache-dir -r requirements.txt

# Option 2:Installer les dépendances dans un environnement virtuel
# pour isoler les packages du système
RUN python -m venv /opt/venv && \
    . /opt/venv/bin/activate && \
    pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Étape 2 : Créer une image finale légère
# cette étape copie l'environnement virtuel et le code pour réduire la taille de l'image
FROM python:3.11-slim

# Créer un utilisateur non-root
RUN useradd -m myuser

# Définir le répertoire de travail
WORKDIR /app

# Copier l'environnement virtuel depuis l'étape build
# minimise l'impact sur le cache
COPY --from=build /opt/venv /opt/venv

# Activer l'environnement virtuel
ENV PATH="/opt/venv/bin:$PATH"

# Copier le reste de l'application
COPY --chown=myuser:myuser . .

# Passer à l'utilisateur non-root
USER myuser

# Exposer le port sur lequel l'application fonctionne
EXPOSE 5000

# Commande pour démarrer l'application
CMD ["gunicorn", "--bind", "0.0.0.0:5000", "app:app"]
