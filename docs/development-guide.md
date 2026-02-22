# Development Guide

Generated: 2026-02-22

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| Node.js | 18.x | See `@types/node: 18.16.9` |
| Yarn | 1.x | `yarn.lock` present |
| .NET SDK | 8.0 | All backend services target net8.0 |
| Docker Desktop | Latest | Required for full backend stack |

---

## Frontend Setup

### Install dependencies
```bash
yarn install
```

### Run applications

```bash
# Main sports-ui app
nx serve sports-ui
# or
yarn start sports-ui

# Admin app
nx serve sports-admin
# or
yarn start sports-admin

# GM app
nx serve sports-gm
```

### Build
```bash
nx build sports-ui
nx build sports-admin
nx build sports-gm
```

### Test
```bash
# Run all unit tests
nx run-many -t test

# Run tests for a specific project
nx test sports-ui
nx test auth-auth-data-access

# Run with coverage
nx run-many -t test --configuration=ci

# Run E2E tests
nx e2e sports-ui-e2e
```

### Lint
```bash
nx run-many -t lint
```

### Generate NSwag API client (after backend API changes)
```bash
yarn gen:sports
# runs: nswag run nswag.sports.json
```

### Generate a new library
```bash
npx nx generate @nx/angular:library --name=<feature-name> --directory=libs/<domain>/<feature-name> --importPath=@sports-ui/<feature-name> --standalone
```

### Generate a new service
```bash
nx g @nx/angular:service <name>.service --project=<library-name>
```

### Generate a new component
```bash
nx g @nx/angular:component libs/<domain>/<library>/src/<component>/<component>
```

---

## Backend Setup

### Option A: Docker Compose (Recommended — full stack)

From the `services/` directory:

```bash
# Start full stack (SQL Server + RabbitMQ + SMTP + all APIs)
docker-compose up

# Start specific services only
docker-compose up sqlserver rabbitmq
```

**Service URLs (Docker):**
| Service | URL |
|---------|-----|
| sportsAPI | http://localhost:5000 |
| Swagger (sportsAPI) | http://localhost:5000/swagger |
| IdentityService | http://localhost:5001 |
| Swagger (Identity) | http://localhost:5001/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| smtp4dev UI | http://localhost:3000 |
| SQL Server | localhost,1433 (sa / Test123!) |

### Option B: Local .NET (services individually)

```bash
# From services/ directory:
cd services/sportsAPI/WebAPI
dotnet run

cd services/IdentityService
dotnet run

cd services/NotificationAPI
dotnet run
```

### Database Migrations

```bash
# From services/ directory:

# sportsAPI — add migration
dotnet ef migrations add <MigrationName> \
  -p .\sportsAPI\Infrastructure\Infrastructure.csproj \
  -s .\sportsAPI\WebAPI\WebAPI.csproj \
  --context SportsDbAppContext

# sportsAPI — apply migrations
dotnet ef database update \
  -p .\sportsAPI\Infrastructure\Infrastructure.csproj \
  -s .\sportsAPI\WebAPI\WebAPI.csproj \
  --context SportsDbAppContext

# IdentityService — add migration
dotnet ef migrations add <MigrationName> \
  -p .\IdentityService\IdentityService.csproj \
  -s .\IdentityService\IdentityService.csproj \
  --context IdentityDbContext

# IdentityService — apply migrations
dotnet ef database update \
  -p .\IdentityService\IdentityService.csproj \
  -s .\IdentityService\IdentityService.csproj \
  --context IdentityDbContext
```

### Run Backend Tests
```bash
dotnet test services/SportsSystem.sln
```

---

## Environment Configuration

### Frontend

Environment config lives in each app's `src/environments/` folder:
- `environment.ts` — local development
- `environment.prod.ts` — production build

Key variables:
```typescript
{
  apiUrl: 'http://localhost:5000',        // sportsAPI base URL
  googleClientId: '<your-google-client-id>'
}
```

### Backend (`appsettings.json` / environment variables)

**sportsAPI (`services/sportsAPI/WebAPI/appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SportsDb;User Id=sa;Password=Test123!;TrustServerCertificate=True;"
  },
  "MessageBroker": {
    "Host": "rabbitmq://rabbitmq",
    "UserName": "guest",
    "Password": "guest"
  },
  "Jwt": {
    "Key": "<your-secret-key>",
    "Issuer": "SportsAPI.IdentityService",
    "Audience": "SportsAPI.Client"
  },
  "IdentityService": {
    "BaseUrl": "http://localhost:5081"
  }
}
```

> ⚠️ **Security**: The default JWT key `YourSuperSecretKeyThatIsAtLeast32CharactersLong!` must be replaced in any non-local environment.

---

## Common Development Tasks

### Adding a new API feature (backend)
1. Add domain entity in `sportsAPI/Domain/`
2. Add DbSet to `SportsDbAppContext` + EF configuration in `Infrastructure/Data/Configurations/`
3. Create migration: `dotnet ef migrations add <Name> ...`
4. Add command/query in `Application/<Feature>/`
5. Add controller endpoint in `WebAPI/Controllers/`
6. Regenerate frontend client: `yarn gen:sports`

### Adding a new frontend feature
1. Generate library: `npx nx generate @nx/angular:library ...`
2. Create Signal Store in `<domain>-data-access`
3. Create smart component(s) in `feature-<name>`
4. Add route in `apps/sports-ui/src/app/app.routes.ts`
5. Register store provider in `app.config.ts` if root-scoped

### Checking affected projects (Nx)
```bash
# See what's affected by current changes
nx affected --target=test
nx affected --target=build
```
