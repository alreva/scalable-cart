# Scalable Cart

Scalable Cart is a sample solution that demonstrates how to build a shopping cart using several .NET technologies. It contains multiple service hosts, a CLI tool and a front‑end application. The projects can be used individually or combined to explore different approaches to implementing the cart domain.

## Projects

| Project | Description |
| ------- | ----------- |
| **CartHost** | Web API using **Akka.NET** actors with SignalR notifications. |
| **CartHost.Marten** | Minimal API that stores events in **Marten** (PostgreSQL). |
| **CartHost.Orleans** | Implementation of the cart using **Orleans** grains. |
| **CatalogManager** | .NET MAUI app to browse and manage catalog data. |
| **Cli** | Command line utility that talks to the Akka.NET cluster. |
| **CartUi** | A [Next.js](https://nextjs.org/) front end located in `CartUi/cart-ui`. |

All C# projects target **.NET 8** and are collected in the `ScalableCart.sln` solution file. The repository also contains xUnit tests in `CartHost.Tests`.

## Building

To build the solution, install the [.NET SDK](https://dotnet.microsoft.com/download) and run:

```bash
dotnet build ScalableCart.sln
```

The Next.js client requires Node.js and can be started from the `CartUi/cart-ui` directory:

```bash
npm install
npm run dev
```

## Running the sample

The cart relies on a PostgreSQL database and several services. The following
steps bring up a local development environment:

1. **Start PostgreSQL** using the provided compose file:

   ```bash
   docker compose -f CartHost.Marten/Startup/docker-compose.yml up -d
   ```

2. **Launch the Marten host** which exposes the cart API on
   `http://localhost:5254`:

   ```bash
   dotnet run --project CartHost.Marten
   ```

3. **Run the Next.js UI** from the `CartUi/cart-ui` folder. The site listens on
   `http://localhost:3000`:

   ```bash
   cd CartUi/cart-ui
   npm install
   npm run dev
   ```

4. **Start the CatalogManager** mobile app (requires the MAUI workload):

   ```bash
   dotnet workload install maui   # first time only
   dotnet run --project CatalogManager
   ```

With all services running you can browse the cart at `http://localhost:3000` and
use the CatalogManager to maintain product information.

## Automating startup

The repository contains a helper script `run-all.sh` that starts PostgreSQL,
CartHost.Marten, the UI and the CatalogManager in one go. Execute:

```bash
./run-all.sh
```

Logs for each component are written to the `logs/` directory.

## Repository Layout

```
CartHost/             # Akka.NET service
CartHost.Marten/      # Marten based service
CartHost.Orleans/     # Orleans based service
CatalogManager/       # MAUI catalog browser
Cli/                  # Command line helper
CartUi/cart-ui/       # Next.js UI
SharedMessages/       # Contracts shared between services
```

This repository is meant for experimentation and learning. Feel free to explore each project to see how different frameworks tackle the cart use case.
