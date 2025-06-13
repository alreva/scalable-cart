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
