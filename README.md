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

### 2) Smoke-check backend

```bash
curl http://localhost:8080/hello
```

Expected response:

```json
{ "message": "Bienvenido a Nido!" }
```

## Notes

- Database bootstrap SQL intentionally contains only database creation (`nido`), aligned with architecture guidance for EF migrations.
- Angular scaffold was generated without dependency installation; run `npm install` inside `frontend/` (or rely on docker compose startup command).
- CI workflow is intentionally minimal for MVP1 base and runs backend/frontend test commands as scaffold targets.
