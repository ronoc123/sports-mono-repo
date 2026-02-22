# Project Documentation Index — sports-ui

Generated: 2026-02-22 | Scan Level: Deep | Mode: initial_scan

---

## Project Overview

- **Type:** Monorepo with 2 parts
- **Primary Languages:** TypeScript (frontend) + C# (backend)
- **Architecture:** Feature-sliced Nx Angular (frontend) + Clean Architecture .NET 8 (backend)
- **Repository:** `C:/Users/kampe/OneDrive/Desktop/sports-ui`

---

## Quick Reference

### Part 1: `frontend` — Angular Nx Workspace
- **Type:** web
- **Tech Stack:** Angular 20, NgRx Signals Store 19, Angular Material, TypeScript 5.8, Nx 21
- **Apps:** sports-ui, sports-admin, sports-gm
- **Root:** `apps/` + `libs/`
- **Entry Point:** `apps/sports-ui/src/main.ts`

### Part 2: `backend` — .NET Microservices
- **Type:** backend
- **Tech Stack:** ASP.NET Core 8, EF Core 8, MassTransit + RabbitMQ, SQL Server 2022
- **Services:** sportsAPI (:5000), IdentityService (:5001), NotificationAPI, MessagingService
- **Root:** `services/`
- **Entry Points:** `services/sportsAPI/WebAPI/Program.cs`, `services/IdentityService/Program.cs`

---

## Generated Documentation

| Document | Description |
|----------|-------------|
| [Architecture](./architecture.md) | Full system architecture — patterns, decisions, security, testing |
| [Source Tree Analysis](./source-tree-analysis.md) | Annotated directory tree for both frontend and backend |
| [Integration Architecture](./integration-architecture.md) | How frontend and backend services communicate |
| [API Contracts — Backend](./api-contracts-backend.md) | All REST API endpoints (sportsAPI + IdentityService) |
| [Data Models — Backend](./data-models-backend.md) | Database schema, domain entities, value objects, migrations |
| [State Management — Frontend](./state-management-frontend.md) | NgRx Signal Stores: shape, methods, computed signals |
| [Component Inventory — Frontend](./component-inventory-frontend.md) | All Angular components by type and feature |
| [Development Guide](./development-guide.md) | Setup, run, build, test, and common dev tasks |

---

## Existing Documentation

| Document | Description |
|----------|-------------|
| [README.md](../README.md) | Angular/Nx architecture overview, component patterns, library organization, run commands |
| [services/README.md](../services/README.md) | Backend services overview (stub) |
| [services/ReadMe.txt](../services/ReadMe.txt) | EF Core migration commands reference |

---

## Getting Started

### Frontend
```bash
yarn install
nx serve sports-ui          # starts on http://localhost:4200
```

### Backend (Docker — recommended)
```bash
cd services
docker-compose up           # starts full stack: API + Identity + DB + RabbitMQ + SMTP
```

Backend URLs:
- sportsAPI Swagger: http://localhost:5000/swagger
- IdentityService Swagger: http://localhost:5001/swagger
- RabbitMQ UI: http://localhost:15672 (guest/guest)
- Email dev UI: http://localhost:3000

---

## For AI-Assisted Development

When working on new features, point your context to the most relevant documents:

| Task | Key Documents |
|------|--------------|
| New frontend feature | `architecture.md`, `state-management-frontend.md`, `component-inventory-frontend.md` |
| New API endpoint (backend) | `architecture.md`, `api-contracts-backend.md`, `data-models-backend.md` |
| New domain entity | `data-models-backend.md`, `architecture.md` |
| Full-stack feature | `integration-architecture.md` + both architecture sections |
| Auth flow changes | `api-contracts-backend.md` (IdentityService), `state-management-frontend.md` (AuthStore) |
| Brownfield PRD | Load `index.md` → then `architecture.md` + `integration-architecture.md` |
