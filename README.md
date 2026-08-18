# NexoDesk

NexoDesk es una mesa de servicio SaaS multi-tenant para gestionar solicitudes
por organización. Cada organización administra sus usuarios, categorías y
solicitudes de forma aislada.

El proyecto incluye una API en .NET y un frontend en Vue. Docker Compose
levanta ambos servicios con SQLite como base de datos.

## Requisitos previos

- Docker Desktop (Windows/macOS) o Docker Engine (Linux), con Docker Compose.
- Solo para ejecutar fuera de Docker: SDK de .NET 10 para el backend y Node.js
  24 con Vite 8 para el frontend.

## Cómo levantar el proyecto

1. Clona el repositorio y entra a su carpeta.

   ```bash
   git clone <URL_DEL_REPOSITORIO>
   cd nexodesk
   ```

2. Crea el archivo de variables de entorno desde el ejemplo.

   Windows PowerShell:

   ```powershell
   Copy-Item .env.example .env
   ```

   Linux/macOS:

   ```bash
   cp .env.example .env
   ```

3. Construye e inicia los servicios.

   ```bash
   docker compose up -d --build
   ```

Comandos útiles:

```bash
docker compose ps
docker compose logs
docker compose down
```

SQLite se crea automáticamente. Al iniciar, EF Core aplica las migraciones y
los datos semilla se crean automáticamente cuando la base está vacía.

## Acceso al sistema

- Frontend: http://localhost:5173
- API: http://localhost:5080
- Swagger: http://localhost:5080/swagger
- Health: http://localhost:5080/api/v1/health

## Credenciales de prueba

Contraseña para todos los usuarios: `NexoDesk.2026`

| Usuario | Organización | Rol |
|---|---|---|
| admin@norte.test | Cooperativa Norte | Admin |
| agente1@norte.test | Cooperativa Norte | Agente |
| agente2@norte.test | Cooperativa Norte | Agente |
| user1@norte.test | Cooperativa Norte | Solicitante |
| user2@norte.test | Cooperativa Norte | Solicitante |
| admin@sur.test | Bufete Sur | Admin |
| user1@sur.test | Bufete Sur | Solicitante |

## Pruebas

```bash
dotnet test backend/tests/Unitarios/NexoDesk.sln --configuration Release
```

Para comprobar los tipos del frontend:

```bash
cd frontend
npm ci
npx vue-tsc --noEmit
```

## Funcionalidades implementadas

- Autenticación JWT.
- Multi-tenancy con aislamiento por organización.
- Gestión de solicitudes y flujo de estados.
- Cálculo automático de SLA.
- Roles y permisos.
- Filtros, búsqueda y paginación server-side.
- Swagger, seed automático y pruebas unitarias.

## Funcionalidades no implementadas

Actualmente no existen funcionalidades pendientes fuera del alcance definido en la prueba técnica.

## Notas importantes

- No versionar archivos `.env`.
- No versionar la base SQLite ni sus archivos asociados.
- Cambiar `JWT_SECRET` por un secreto seguro en ambientes reales.
