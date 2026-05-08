# Nido — MVP1 repository base

This repository now contains the **MVP1 base architecture**.

## Monorepo structure

```txt
.
├─ Arquitectura-MVP1.md
├─ backend/
│  ├─ Nido.slnx
│  ├─ src/
│  │  ├─ Nido.Api/
│  │  ├─ Nido.Application/
│  │  ├─ Nido.Domain/
│  │  └─ Nido.Infrastructure/
│  └─ tests/
│     ├─ Nido.Domain.Tests/
│     ├─ Nido.Application.Tests/
│     └─ Nido.Api.IntegrationTests/
├─ frontend/
│  ├─ src/app/core/
│  ├─ src/app/features/
│  │  ├─ home/
│  │  └─ household/create-household/
│  ├─ src/app/shared/ui/
│  ├─ src/environments/
│  └─ Dockerfile
├─ docker/
│  └─ mysql/init.sql
├─ .github/workflows/ci.yml
└─ docker-compose.yml
```

## Local startup intention

### 1) Start local stack

```bash
docker compose up --build
```

Expected services:
- Frontend: `http://localhost:4200`
- Backend API: `http://localhost:8080`
- MySQL: `localhost:3306`

### 2) Daily development flow

#### Happy path

Start the stack once:

```bash
docker compose up --build
```

After that, for normal code changes you usually **do not need to rebuild containers**.

Why:
- `backend/` is mounted as a volume into the container;
- the backend runs with `dotnet watch`;
- `frontend/` is mounted as a volume into the container;
- the frontend runs with the Angular dev server.

That means:
- backend code changes should be picked up automatically;
- frontend code changes should reload automatically in the browser.

#### When a rebuild is needed

Rebuild/recreate containers when you change infrastructure or dependency setup, for example:

- `docker-compose.yml`
- Dockerfiles
- dependency manifests such as `frontend/package.json`
- anything that changes the container image or startup command

Use:

```bash
docker compose up -d --build
```

If you only need to recreate one service:

```bash
docker compose up -d --build backend
docker compose up -d --build frontend
```

### 3) Smoke-check backend

```bash
curl http://localhost:8080/hello
```

Expected response:

```json
{ "message": "Bienvenido a Nido!" }
```

### 4) Browser check

Open `http://localhost:4200`.

Expected behavior:
- the frontend calls `GET /hello` automatically;
- the page shows the backend welcome message;
- if the backend is not reachable or CORS is misconfigured, the page will show an error state.

## Notes

- Database bootstrap SQL intentionally contains only database creation (`nido`), aligned with architecture guidance for EF migrations.
- Frontend uses `npm` as the package manager.
- `docker compose` already runs `npm install` for the frontend container on startup.
- CI workflow is intentionally minimal for MVP1 base and runs backend/frontend test commands as scaffold targets.
