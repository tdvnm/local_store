# Society Commerce

A society/apartment store commerce platform with buyer, seller, and admin frontends backed by a .NET 8 API.

## Project Structure

```
society-commerce-api/    # .NET 8 Web API (PostgreSQL + Redis)
society-commerce-web/    # SvelteKit monorepo (pnpm workspaces)
  apps/
    buyer/               # Customer-facing storefront (:5173)
    seller/              # Seller dashboard (:5174)
    admin/               # Admin panel (:5175)
  packages/
    api-client/          # Shared API client + SignalR realtime
    ui/                  # Shared UI component library
    i18n/                # Internationalization
```

## Prerequisites

- **Node.js** >= 22 ([.nvmrc](society-commerce-web/.nvmrc))
- **pnpm** >= 9
- **Docker** & **Docker Compose** (for PostgreSQL + Redis)
- **.NET 8 SDK** (if running API without Docker)

## Getting Started

### 1. Start the API (Docker — recommended)

```bash
cd society-commerce-api
docker compose up -d
```

This spins up **PostgreSQL**, **Redis**, and the **.NET API** on `http://localhost:5000`.

To seed the database:

```bash
# Connect to the running postgres container and run the seed script
docker compose exec postgres psql -U sc_dev -d society_commerce -f /dev/stdin < seed.sql
```

#### Running API without Docker

```bash
cd society-commerce-api
# Make sure PostgreSQL and Redis are running locally
# Connection config is in src/SocietyCommerce.Api/appsettings.json
dotnet run --project src/SocietyCommerce.Api
```

### 2. Start the Web Frontend

```bash
cd society-commerce-web
pnpm install
```

Then run whichever app you need:

```bash
pnpm dev:buyer    # http://localhost:5173
pnpm dev:seller   # http://localhost:5174
pnpm dev:admin    # http://localhost:5175
```

Or build all apps:

```bash
pnpm build
```

## API Configuration

Default dev config is in `society-commerce-api/src/SocietyCommerce.Api/appsettings.json`. For production, override via environment variables:

| Setting | Env Var | Dev Default |
|---|---|---|
| DB connection | `ConnectionStrings__Default` | `Host=localhost;...Password=sc_dev_pass` |
| Redis | `ConnectionStrings__Redis` | `localhost:6379` |
| JWT secret | `Jwt__Secret` | `dev-secret-key-min-32-characters-long-here` |

**Never use the dev defaults in production.** Set real secrets via environment variables or a secrets manager.
