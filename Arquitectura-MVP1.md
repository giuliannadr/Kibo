# Nido — Arquitectura técnica del MVP 1

Este documento describe la base técnica definida para el primer MVP de **Nido**. El objetivo de esta etapa no es maximizar la cantidad de funcionalidades, sino establecer una estructura sólida sobre la cual el producto pueda crecer con orden, mantenibilidad y capacidad de prueba.

La propuesta técnica del MVP 1 prioriza cinco ejes: **arquitectura**, **separación de responsabilidades**, **calidad automática**, **entorno reproducible** y **primer flujo real validable**.

---

## 1. Decisión arquitectónica principal

Para el MVP 1, Nido adopta la siguiente estrategia:

> **Monorepo + monolito modular + Clean Architecture en backend + Angular feature-first en frontend.**

### Justificación

Nido cubre varios módulos funcionales, todos forman parte de un mismo dominio cohesivo: la gestión colaborativa del hogar. Por ese motivo, un monolito modular resulta más adecuado para la primera versión.

Este enfoque permite:

- mantener una única base de código;
- separar claramente responsabilidades sin fragmentar el sistema demasiado pronto;
- acelerar el desarrollo inicial;
- reducir el costo de pruebas, despliegue y mantenimiento.

---

## 2. Alcance técnico del MVP 1

El MVP 1 debe demostrar estas capacidades mínimas:

- Backend .NET operativo.
- Frontend Angular operativo.
- Base de datos MySQL disponible mediante Docker.
- `docker compose` para levantar el entorno local.
- CI ejecutando al menos un test de backend y uno de frontend.
- Endpoint simple `GET /hello` para validar integración base.
- Primer caso de uso real: `POST /household` para crear un hogar.

En esta etapa no se priorizan autenticación ni funcionalidades complejas de producto. El foco está en consolidar la base técnica del sistema: estructura, entorno reproducible, pruebas automáticas y primer flujo real testeable.

---

## 3. Convenciones de nombres

Todo el código se escribe en inglés: clases, carpetas, endpoints, contratos HTTP, tests y nombres de métodos.

Aunque el concepto de negocio es **hogar**, en código se representa como `Household` para mantener consistencia técnica y semántica.

---

## 4. Alineación tecnológica

### 4.1 Backend — .NET 10

La solución del proyecto utiliza el formato actual:

```txt
Nido.slnx
```

Esto mantiene al repositorio alineado con las convenciones modernas del ecosistema .NET.

### 4.2 Frontend — Angular 21

El frontend sigue convenciones actuales de Angular CLI:

- aplicación bajo `src/app`;
- ambientes bajo `src/environments`;
- componentes standalone por defecto;
- strict mode habilitado;
- tests unitarios habilitados desde el inicio;
- Vitest como test runner;
- organización feature-first.

Esta elección favorece una base moderna, mantenible y alineada con el tooling actual del framework.

---

## 5. Estructura del repositorio

### 5.1 Vista general

| Área | Contenido |
|------|-----------|
| `backend/` | API .NET, dominio, casos de uso, infraestructura y tests |
| `frontend/` | Aplicación Angular organizada por features |
| `docker/` | Setup local de MySQL |
| `.github/workflows/` | Pipelines de integración continua |
| `docker-compose.yml` | Orquestación local del entorno |

### 5.2 Árbol resumido

```txt
nido/
├─ backend/
│  ├─ Nido.slnx
│  ├─ src/
│  │  ├─ Nido.Api/             → controllers, contracts, Program.cs
│  │  ├─ Nido.Application/     → casos de uso y puertos
│  │  ├─ Nido.Domain/          → entidades, value objects, domain services
│  │  └─ Nido.Infrastructure/  → EF Core, repositorios, providers
│  └─ tests/
│     ├─ Nido.Domain.Tests/
│     ├─ Nido.Application.Tests/
│     └─ Nido.Api.IntegrationTests/
├─ frontend/
│  ├─ src/app/
│  │  ├─ core/                 → cliente API y configuración transversal
│  │  ├─ features/             → módulos funcionales
│  │  └─ shared/ui/            → componentes reutilizables
│  ├─ src/environments/
│  └─ Dockerfile
├─ docker/mysql/init.sql
├─ .github/workflows/
├─ docker-compose.yml
└─ README.md
```

---

## 6. Organización del backend

La arquitectura del backend separa responsabilidades en cuatro proyectos principales.

```txt
Nido.Api
  → expone HTTP
Nido.Application
  → orquesta casos de uso
Nido.Domain
  → modela reglas de negocio puras
Nido.Infrastructure
  → implementa persistencia e integraciones externas
```

### 6.1 `Nido.Domain`

Representa el núcleo del negocio. No depende de HTTP, Entity Framework, MySQL, JWT ni Angular.

Incluye:

- **Entidades y value objects**
  - `Household`
  - `HouseholdName`
- **Lógica de negocio pura**
- **Domain services**, solo cuando una regla de negocio debe reutilizarse entre más de un caso de uso.

La regla principal es que el dominio debe poder entenderse y probarse sin depender de detalles técnicos.

### 6.2 `Nido.Application`

Contiene los casos de uso y las abstracciones que estos necesitan para ejecutarse.

Ejemplos:

- `CreateHouseholdHandler`
- `JoinHouseholdHandler`
- `IHouseholdRepository`

Su responsabilidad es coordinar el flujo de aplicación sin incorporar detalles de infraestructura.

### 6.3 `Nido.Infrastructure`

Agrupa los detalles técnicos reemplazables, especialmente:

- persistencia con Entity Framework y MySQL;
- repositorios concretos;
- configuraciones de mapeo;
- clientes de APIs externas.

Esta separación permite cambiar herramientas técnicas sin alterar el dominio.

### 6.4 `Nido.Api`

Es la capa de exposición HTTP del sistema. Contiene controllers, middlewares, configuración e inyección de dependencias.

Los controllers deben mantenerse delgados: reciben requests, invocan casos de uso y devuelven respuestas.

---

## 7. Gestión del esquema de base de datos

El esquema de base de datos se administra exclusivamente con **EF Core Migrations**.

No se utiliza `init.sql` para definir tablas o relaciones, ya que eso generaría duplicación de fuentes de verdad y conflicto con las migraciones.

El archivo `docker/mysql/init.sql` queda limitado a la creación de la base vacía:

---

## 8. Flujos iniciales del MVP 1

### 8.1 Flujo de integración base

#### Endpoint

```http
GET /hello
```

#### Respuesta

```json
{ "message": "Bienvenido a Nido!" }
```

#### Propósito

Este endpoint no representa lógica de negocio. Su función es validar que:

- el backend responde correctamente;
- el frontend puede consumir la API;
- Docker levanta el entorno;
- el pipeline puede ejecutar verificaciones mínimas.

### 8.2 Primer caso de uso real

#### Endpoint

```http
POST /household
```

#### Request

```json
{ "name": "Casa de Nico" }
```

#### Response

```json
{ "id": "uuid", "name": "Casa de Nico" }
```

#### Valor arquitectónico

Este flujo permite validar de forma concreta:

- `Household` como entidad de dominio;
- `HouseholdName` como value object;
- `CreateHouseholdHandler` como caso de uso;
- `IHouseholdRepository` como puerto;
- `EfHouseholdRepository` como implementación de infraestructura;
- pruebas unitarias e integración sobre un flujo real.

---

## 9. Estrategia de testing

El dominio es la parte más crítica del sistema y debe estar completamente cubierto por pruebas unitarias.

La estrategia de trabajo se basa en **TDD estricto**: primero se escribe el test, luego la implementación y finalmente el refactor.

Todos los tests deben seguir la estructura **Given / When / Then**.

### 9.1 Tipos de tests

**`Nido.Domain.Tests` y `Nido.Application.Tests` — pruebas unitarias**

Prueban clases en aislamiento. No utilizan base de datos, HTTP ni detalles de infraestructura. Las dependencias externas se reemplazan por mocks o dobles de prueba.

**`Nido.Api.IntegrationTests` — pruebas de integración**

Validan el flujo completo contra una base real levantada con Docker. Usan `WebApplicationFactory` de .NET para comprobar que endpoint, caso de uso, persistencia y respuesta funcionan end-to-end.

**Pruebas de interfaz y exploratorias**

Se incorporan cuando existan pantallas funcionales. No forman parte del alcance técnico inicial del MVP 1.

---

## 10. Integración continua

La integración continua se configura desde el comienzo del proyecto.

Su función en esta etapa es asegurar que la base técnica se mantenga estable desde el primer commit, aun cuando la lógica de negocio todavía sea mínima. Por eso, el pipeline debe ejecutar verificaciones automáticas de backend y frontend desde el inicio.

---

## 11. Flujo de trabajo en Git

- Cada feature se desarrolla en su propia **feature branch**.
- Los hotfixes se realizan sobre `main`.
- Los commits deben ser frecuentes y representar unidades de trabajo claras.
- La rama `main` debe mantenerse estable y con CI en verde.

---

## 12. Reglas de calidad

- No crear carpetas genéricas como `Utils`, `Helpers` o `Common` sin una responsabilidad concreta.
- Todo código de dominio debe tener pruebas unitarias.
- Los tests deben seguir estructura `Given / When / Then`.
- El dominio no depende de infraestructura; la infraestructura sí puede depender del dominio.
- CI debe existir desde el inicio del proyecto.
- Docker debe permitir levantar el entorno local de forma reproducible.
- El MVP 1 prioriza base técnica antes que volumen de funcionalidades.

---

## 13. Criterios de éxito del MVP 1 técnico

- [x] `docker compose up` levanta backend, frontend y base de datos.
- [x] Angular consume `GET /hello` y muestra el mensaje.
- [ ] La API permite crear el hogar del usuario con `POST /household`.
- [ ] El dominio de `Household` tiene tests unitarios.
- [ ] El caso de uso `CreateHousehold` tiene tests unitarios.
- [x] El pipeline de CI ejecuta los tests automáticamente.
- [x] La estructura permite distinguir claramente dominio e infraestructura.

---

## 14. Conclusión

El MVP 1 técnico de Nido busca consolidar una base de desarrollo clara, mantenible y extensible. La prioridad no está en sumar muchas funcionalidades desde el inicio, sino en establecer una estructura capaz de sostener el crecimiento del producto sin perder orden arquitectónico.

En este contexto, la combinación de monorepo, monolito modular, Clean Architecture, Angular feature-first, Docker y CI ofrece una base equilibrada entre simplicidad operativa, calidad técnica y capacidad de evolución.
