#!/bin/bash
set -e

SERVICES="AuthService ProfileService JobService ApplicationService AIService ApiGateway"

echo "Publishing all services..."
for svc in $SERVICES; do
    echo "  Building $svc..."
    dotnet publish "src/services/$svc/$svc.csproj" -c Release -o "src/services/$svc/publish" --verbosity quiet
done

echo "Building Docker images..."
docker-compose build

echo "Done."
