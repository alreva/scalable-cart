#!/usr/bin/env bash
set -e

# Directory of this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

mkdir -p logs

# Build .NET projects
if ! dotnet --version &>/dev/null; then
  echo "dotnet CLI is required" >&2
  exit 1
fi

dotnet build ScalableCart.sln

# Start PostgreSQL container
if command -v docker >/dev/null; then
  docker compose -f CartHost.Marten/Startup/docker-compose.yml up -d
else
  echo "Docker is required to start PostgreSQL" >&2
  exit 1
fi

# Start CartHost.Marten
dotnet run --project CartHost.Marten > logs/carthost.log 2>&1 &
CART_HOST_PID=$!

echo "CartHost.Marten running with PID ${CART_HOST_PID}" 

# Start Cart UI
pushd CartUi/cart-ui >/dev/null
npm install
npm run dev > ../../logs/cartui.log 2>&1 &
UI_PID=$!
popd >/dev/null

echo "Cart UI running with PID ${UI_PID} at http://localhost:3000"

# Start CatalogManager MAUI app
dotnet run --framework net8.0-maccatalyst --project CatalogManager > logs/catalogmanager.log 2>&1 &
CAT_PID=$!

echo "CatalogManager running with PID ${CAT_PID}"

echo "Logs are written to the logs/ directory."
