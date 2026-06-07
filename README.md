# TaskFlow

A full-stack **Kanban board** application — React + TypeScript frontend, ASP.NET Core
(.NET 10) Web API backend, PostgreSQL, JWT authentication.

> Status: 🚧 v0.1 — auth + boards/columns/cards CRUD working end-to-end, tested.

## Stack

| Layer | Tech |
|---|---|
| Frontend | React 19, TypeScript, Vite, native HTML5 drag-and-drop |
| Backend | ASP.NET Core 10 Web API, EF Core 10, controllers |
| Database | PostgreSQL (EF Core migrations) |
| Auth | JWT bearer tokens, BCrypt password hashing |
| Docs | Swagger / OpenAPI |
| Tests | xUnit (12 tests: auth, JWT, board/card logic, ownership) |
| Ops | Docker Compose (api + postgres) |

## Architecture

```
frontend (React/TS, :5173)  ──HTTP/JWT──>  backend API (ASP.NET Core, :5080)  ──EF Core──>  PostgreSQL
```

Backend layers: `Controllers → AppDbContext (EF Core) → PostgreSQL`, with
`Services` for password hashing and JWT issuance. Every board/column/card query is
scoped to the authenticated owner, so users only ever see their own data.

## Features

- Register / login with hashed passwords and JWT sessions
- Create boards (auto-seeded with **To Do / In Progress / Done** columns)
- Add custom columns; add, edit, delete cards
- **Drag-and-drop** cards between columns
- Per-user data isolation enforced on every endpoint

## Run it

### Option A — Docker (full stack + Postgres)

```bash
docker compose up --build
# API on http://localhost:5080  (Swagger at /swagger)
```

### Option B — local dev

Backend (needs PostgreSQL, or use the zero-dependency in-memory mode):

```bash
cd backend/TaskFlow.Api
# with Postgres (connection string in appsettings.json):
dotnet run
# OR no database required (data lives in memory for the session):
USE_INMEMORY_DB=true dotnet run
```

Frontend:

```bash
cd frontend
cp .env.example .env     # points at http://localhost:5080/api
npm install
npm run dev              # http://localhost:5173
```

## API

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/register` | Create account, returns JWT |
| POST | `/api/auth/login` | Authenticate, returns JWT |
| GET | `/api/auth/me` | Current user (auth) |
| GET | `/api/boards` | List my boards |
| POST | `/api/boards` | Create board (seeds 3 columns) |
| GET | `/api/boards/{id}` | Board with columns + cards |
| DELETE | `/api/boards/{id}` | Delete board |
| POST | `/api/boards/{id}/columns` | Add column |
| POST | `/api/columns/{id}/cards` | Add card |
| PUT | `/api/cards/{id}` | Edit card |
| PUT | `/api/cards/{id}/move` | Move card to a column/order |
| DELETE | `/api/cards/{id}` | Delete card |

## Tests

```bash
cd backend
dotnet test
```

Covers password hashing (salting + verification), JWT claims/expiry, default-column
seeding, owner-scoped listing, and cross-user access being rejected.

## Notes

- JWT signing key and DB credentials are read from config / environment
  (`Jwt__Key`, `ConnectionStrings__Default`) — override them in any real deployment.
- EF Core migrations live in `backend/TaskFlow.Api/Data/Migrations`.
