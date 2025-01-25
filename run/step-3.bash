#!/usr/bin/env bash

# Étape 3 - PostgreSQL

docker run \
    --rm \
    -p 5432:5432 \
    -e POSTGRES_USER=postgres \
    -e POSTGRES_PASSWORD=postgres \
    -e POSTGRES_DB=postgres \
    postgres
