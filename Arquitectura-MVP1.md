# Nido — Arquitectura técnica para MVP 1

Este documento define la estructura inicial de código para el primer MVP de **Nido**. El objetivo no es construir muchas funcionalidades, sino dejar una base técnica defendible: CI, Docker, backend .NET 10, frontend Angular 21, separación dominio/infraestructura y un primer caso de uso real testeable.

---

## Decisión principal

Para el MVP 1, Nido usará:

> **Monorepo + Monolito Modular + Clean Architecture en backend + Angular feature-first en frontend.**

No se usará microservicios en esta etapa.

### Por qué no microservicios

Microservicios agregarían complejidad operativa sin una necesidad real todavía: múltiples despliegues, comunicación entre servicios, observabilidad distribuida, autenticación distribuida y mayor dificultad para probar el sistema completo.

Nido tiene muchos módulos funcionales, pero todos pertenecen a un mismo dominio cohesivo: la gestión colaborativa de un hogar.

---

## Alcance técnico del MVP 1

El primer MVP tiene que demostrar estas capacidades mínimas:

- Backend .NET levantando correctamente.
- Frontend Angular levantando correctamente.
- MySQL disponible mediante Docker.
- `docker compose` para levantar el entorno local.
- CI ejecutando al menos un test de backend y uno de frontend.
- Endpoint simple `GET /hello` para probar integración.
- Primer caso de uso real: `POST /household` para crear el hogar del usuario.

Auth y features de producto se incorporan a partir del siguiente MVP. El foco del MVP 1 es establecer la base de calidad: arquitectura, CI, Docker y el primer flujo real testeado.

---

## Nota sobre nombres:

Todo el código se escribe en inglés: clases, carpetas, endpoints, contratos HTTP, tests y nombres de métodos.

El concepto de negocio en español es **hogar**, pero en código se representa como `Household`.

---

## Alineación con .NET 10 y Angular 21

### .NET 10

La solución del proyecto usa el formato nuevo:

```txt
Nido.slnx
```

Con el SDK actual de .NET, `dotnet new sln` genera `.slnx`. Si existiera una solución vieja `.sln`, se migra con:

```bash
dotnet sln migrate
```

### Angular 21

El frontend sigue convenciones actuales de Angular CLI:

- Aplicación bajo `src/app`.
- Configuración de ambientes bajo `src/environments`.
- Componentes standalone por defecto.
- Strict mode activo.
- Tests unitarios generados por defecto.
- Vitest como test runner por defecto.
- File naming style actual: evitar el sufijo mecánico `.component` en los nombres de archivos.

---

## Estructura del repositorio

### Vista rápida

| Área | Qué contiene |
|------|---------------|
| `backend/` | API .NET, dominio, casos de uso, infraestructura y tests |
| `frontend/` | App Angular feature-first |
| `docker/` | Setup local de MySQL |
| `.github/workflows/` | Pipelines de CI |
| `docker-compose.yml` | Orquestación local del entorno |

<div style="page-break-before: always;"></div>

### Árbol resumido

```txt
nido/
├─ backend/
│  ├─ Nido.slnx
│  ├─ src/
│  │  ├─ Nido.Api/             → controllers, contracts, Program.cs
│  │  ├─ Nido.Application/     → use cases + puertos
│  │  ├─ Nido.Domain/          → entidades, value objects, domain services
│  │  └─ Nido.Infrastructure/  → EF Core, repositorios, providers
│  └─ tests/
│     ├─ Nido.Domain.Tests/
│     ├─ Nido.Application.Tests/
│     └─ Nido.Api.IntegrationTests/
├─ frontend/
│  ├─ src/app/
│  │  ├─ core/                 → cliente API y configuración transversal
│  │  ├─ features/             → home, household/create-household
│  │  └─ shared/ui/            → componentes reutilizables
│  ├─ src/environments/
│  └─ Dockerfile
├─ docker/mysql/init.sql
├─ .github/workflows/
├─ docker-compose.yml
└─ README.md
```

### Zoom a backend

```txt
Nido.Api
  → expone HTTP
Nido.Application
  → orquesta casos de uso (`CreateHousehold`, `JoinHousehold`)
Nido.Domain
  → modela negocio puro (`Household`, `HouseholdName`, `HouseholdService`)
Nido.Infrastructure
  → implementa persistencia e integraciones (`EfHouseholdRepository`, EF Core)
```

---

## Backend: separación de responsabilidades

### `Nido.Domain`

Modela el problema de negocio. No conoce HTTP, Entity Framework, MySQL, JWT ni Angular.

Contiene tres tipos de componentes:

**Model** — entidades y value objects que representan el dominio:
```txt
Household
HouseholdName
```

**Use Case** — cada feature de la aplicación es una clase con una única responsabilidad. Un handler por caso de uso, nunca un service con múltiples métodos que crece sin control:
```txt
CreateHouseholdHandler
JoinHouseholdHandler
```

**Domain Service** — lógica de negocio que es necesaria en más de un caso de uso. Si `CreateHousehold` y `JoinHousehold` comparten una validación del nombre del hogar, esa lógica se extrae a `HouseholdService` en el dominio — no se duplica en los handlers:
```txt
HouseholdService
```

La regla: el Domain Service existe solo cuando hay lógica de negocio real que se repetiría en múltiples handlers. No se crea por anticipación.

### `Nido.Application`

Contiene los casos de uso y las abstracciones (interfaces de repositorios) que esos casos de uso necesitan. La interfaz `IHouseholdRepository` vive junto al módulo `Household`, no en una carpeta global de abstracciones que se convierte en un cajón de sastre.

### `Nido.Infrastructure`

Contiene detalles técnicos reemplazables, divididos en dos responsabilidades:

- **Persistence** — Entity Framework, MySQL, repositorios concretos, configuraciones de mapeo.
- **Providers** — clientes de APIs externas (Open Food Facts, integraciones futuras). Cada cliente externo tiene su propio DTO de mapeo, separado del dominio.

### `Nido.Api`

Expone la aplicación al mundo exterior mediante HTTP. Contiene controllers, configuración del servidor, inyección de dependencias y middlewares. Los controllers son delgados: reciben el request, invocan el handler y devuelven la respuesta.

---

## Schema de base de datos: EF Migrations

El schema de la base de datos se gestiona exclusivamente con **EF Core Migrations**. No se usa `init.sql` para definir el schema — eso entraría en conflicto con las migrations al primer cambio de modelo.

El `docker/mysql/init.sql` se limita a crear la base de datos vacía:
```sql
CREATE DATABASE IF NOT EXISTS nido;
```

Las migrations se aplican al startup en desarrollo:
```csharp
// Program.cs
app.Services.GetRequiredService<NidoDbContext>().Database.Migrate();
```

---

## Flujo 1: endpoint de integración

### Endpoint

```http
GET /hello
```

### Respuesta

```json
{ "message": "Bienvenido a Nido!" }
```

### Para qué sirve

Este endpoint no representa lógica de negocio. Sirve para validar que el backend responde, el frontend puede consumirlo, Docker levanta correctamente y el pipeline puede ejecutar tests mínimos.

---

## Flujo 2: primer caso de uso real

### Endpoint

```http
POST /household
```

### Request

```json
{ "name": "Casa de Nico" }
```

### Response

```json
{ "id": "uuid", "name": "Casa de Nico" }
```

### Qué permite defender

- `Household` como entidad de dominio.
- `HouseholdName` como value object.
- `CreateHouseholdHandler` como caso de uso.
- `HouseholdService` como dominio puro si hay validaciones compartidas.
- `IHouseholdRepository` como puerto.
- `EfHouseholdRepository` como implementación de infraestructura.
- Tests unitarios del dominio, del caso de uso y del endpoint HTTP.

---

## Tests

El dominio es lo más crítico y debe estar testeado al 100%. Se trabaja con TDD estricto: el test se escribe antes que la implementación, seguido del refactor.

Todos los tests siguen la estructura **Given / When / Then** y deben ser claros sin necesidad de comentarios explicativos.

### Tipos de tests por proyecto

**`Nido.Domain.Tests` y `Nido.Application.Tests` — Pruebas unitarias**

Prueban una única clase en aislamiento. No tocan base de datos, HTTP ni ningún detalle de infraestructura. Los repositorios se mockean.

**`Nido.Api.IntegrationTests` — Pruebas de integración**

Prueban el flujo completo contra una base de datos real levantada con Docker. Usan `WebApplicationFactory` de .NET. Validan que el endpoint HTTP, el handler, la persistencia y la respuesta funcionan end-to-end.

**Tests de GUI y exploratorios**

Se incorporan una vez que existen pantallas reales. No son parte del MVP 1 técnico.

---

## CI

El pipeline de CI se configura desde el primer commit, antes de tener lógica de negocio. El primer paso concreto es agregar un test que siempre pase — esto valida que el pipeline corre correctamente sin depender de lógica real aún.

El pipeline se puede ejecutar localmente antes de pushear para no romper main.

---

## Git workflow

- Cada feature se desarrolla en su propia **feature branch**.
- Los hotfixes se hacen directamente sobre `main`.
- No se acumula código sin pushear — commits frecuentes y granulares.
- `main` siempre debe estar en verde (CI pasando).

---

## Reglas de calidad

- No crear carpetas llamadas `Utils`, `Helpers` o `Common` sin una responsabilidad clara y nombrada.
- Todo código de dominio debe tener pruebas unitarias.
- Los tests siguen estructura `Given / When / Then`.
- El dominio no depende de infraestructura. La infraestructura puede depender del dominio.
- CI existe desde el inicio, aunque al principio ejecute un test mínimo.
- Docker permite levantar el entorno local de forma reproducible.
- El MVP 1 prioriza base técnica antes que cantidad de features.

---

## Criterio de éxito del MVP 1 técnico

- [ ] `docker compose up` levanta backend, frontend y base de datos.
- [ ] Angular consume `GET /hello` y muestra el mensaje.
- [ ] La API permite crear el hogar del usuario con `POST /household`.
- [ ] El dominio de `Household` tiene tests unitarios.
- [ ] El caso de uso `CreateHousehold` tiene tests unitarios.
- [ ] El pipeline de CI corre los tests automáticamente.
- [ ] La estructura permite explicar claramente qué es dominio y qué es infraestructura.

---

## Frase de defensa

> En el MVP 1 no buscamos funcionalidad compleja, buscamos establecer la base de calidad del producto: CI, Docker, separación entre dominio e infraestructura, primer caso de uso real testeado y comunicación frontend-backend. Esto nos permite crecer sin perder orden arquitectónico.
