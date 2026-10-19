# Guía del proyecto — Sistema Académico

Esta guía describe la estructura que existe actualmente en el repositorio, el propósito de sus carpetas y archivos principales, cómo ejecutar el sistema y cómo incorporar cambios sin confundir funcionalidades reales con pantallas de demostración.

> El nombre del repositorio es **Sistema-Academico**. Esta documentación se refiere a ese proyecto.

## 1. Qué es el sistema

Aplicación web para el Instituto Superior Cura Gabriel Brochero. Está pensada para dos perfiles:

- **Secretario académico:** gestionar personas, docentes, planes de estudio e inscripciones.
- **Estudiante:** consultar su actividad e inscribirse a materias.

La solución tiene tres partes:

1. **Frontend:** React y Vite, dentro de `src/`.
2. **Backend:** API REST con ASP.NET Core y .NET 10, dentro de `backend/`.
3. **Base de datos:** SQL Server, definida en `database/AcademicSystem.sql`.

El repositorio también contiene las reglas de colaboración, documentación funcional y configuración de integración continua.

## 2. Estado actual: real vs. simulado

Conviene identificar esta diferencia antes de desarrollar una funcionalidad:

- El backend tiene autenticación con JWT, consulta de salud de la API/base y operaciones de administración de usuarios, roles, estudiantes y secretarios académicos.
- El formulario de login del frontend llama a `POST /api/auth/login`.
- Varias pantallas visibles ya tienen interacción y datos de ejemplo, pero todavía **no están conectadas a endpoints reales**. Planes, docentes e inscripciones usan datos locales o repositorios simulados; algunas acciones solo muestran avisos.
- Los endpoints de materias, inscripciones académicas, planes y docentes no aparecen implementados actualmente en los controllers del backend.
- La base contiene el esquema académico más amplio que necesitarán esas funciones; que una tabla exista no significa que el endpoint o la pantalla ya estén conectados.

Antes de extender un módulo, revisá tanto su pantalla como el API/repository asociado para confirmar de dónde vienen los datos.

## 3. Mapa de carpetas y archivos

El mapa muestra el contenido versionado relevante; omite `.git/` y carpetas generadas como `node_modules/`, `dist/`, `bin/` y `obj/`.

```text
Sistema-Academico/
├── .github/
│   ├── workflows/ci.yml
│   ├── PULL_REQUEST_TEMPLATE.md
│   ├── PULL_REQUEST_TEMPLATE/
│   │   ├── promocion-a-testing.md
│   │   └── promocion-a-production.md
│   └── CODEOWNERS
├── backend/
│   ├── .gitignore
│   ├── AcademicSystem.sln
│   ├── dotnet-tools.json
│   ├── AcademicSystem.Api/
│   │   ├── Controllers/
│   │   ├── Security/
│   │   ├── Properties/
│   │   ├── AcademicSystem.Api.csproj
│   │   ├── Program.cs
│   │   ├── DevelopmentSeeder.cs
│   │   ├── AcademicSystem.Api.http
│   │   └── appsettings*.json
│   ├── AcademicSystem.Business/
│   │   └── Services/
│   ├── AcademicSystem.Data/
│   │   ├── Context/
│   │   └── Repositories/
│   └── AcademicSystem.Entities/
│       ├── DTOs/
│       └── Models/
├── database/
│   └── AcademicSystem.sql
├── documentos/
│   ├── guia-del-proyecto.md
│   ├── Sistema Académico.md
│   ├── Equipos Proyectos 2do.md
│   ├── continuacion.md
│   ├── diagrama_uml_Sistema_Académico.md
│   ├── flujo-git.md
│   ├── integracion-y-releases.md
│   └── configuracion-github.md
├── src/
│   ├── api/
│   ├── assets/
│   ├── core/
│   ├── data/
│   ├── hooks/
│   ├── modules/
│   │   ├── auth/
│   │   ├── estudiante/
│   │   └── secretario/
│   ├── styles/
│   └── main.jsx
├── .env.example
├── .gitattributes
├── .gitignore
├── .prettierrc
├── eslint.config.js
├── index.html
├── package.json
├── package-lock.json
├── vite.config.js
└── vitest.config.js
```

### Archivos de la raíz

| Archivo | Propósito |
|---|---|
| `README.md` | Incorporación al proyecto, instalación local, flujo Git, base de datos y preguntas frecuentes. |
| `package.json` / `package-lock.json` | Dependencias, scripts y versiones reproducibles del frontend. |
| `index.html` | Documento HTML de entrada de Vite; incluye el elemento donde React monta la aplicación. |
| `vite.config.js` | Configuración del servidor y plugin de React. |
| `vitest.config.js` | Configuración de Vitest; busca pruebas `*.test.js` y `*.test.jsx` dentro de `src/`. |
| `eslint.config.js` | Reglas de análisis estático para JavaScript y JSX. |
| `.prettierrc` | Preferencias de formato del código. |
| `.env.example` | Ejemplo de variables públicas del frontend. Copiar a `.env.local` para configurar la máquina local; no subir secretos ni archivos locales de entorno. |
| `.gitignore` | Excluye dependencias, builds, cachés y archivos de configuración local. |
| `.gitattributes` | Normaliza atributos de archivos bajo control de versiones. |

### `src/`: frontend

#### Entrada y estilos globales

- `src/main.jsx`: importa los estilos globales, encuentra `#root` y renderiza `<App />` bajo `StrictMode`.
- `src/assets/icgb-logo.svg`: logotipo institucional.
- `src/styles/`: hojas de estilo organizadas por alcance:
  - `index.css`: punto de entrada de los estilos.
  - `tokens.css`: variables visuales/tokens.
  - `globals.css`, `base.css`: reglas globales y base.
  - `layout.css`, `navigation.css`: estructura, encabezado y navegación.
  - `login.css`: acceso.
  - `management.css`: pantallas de gestión/tablas.
  - `plans.css`: planes de estudio.
  - `enrollments.css`: inscripciones.
  - `student.css`: vistas del estudiante.

#### `src/core/`: infraestructura compartida de la interfaz

- `routes/App.jsx`: router principal, control de acceso, protección por perfil y pantalla de acceso denegado.
- `auth/session.js`: guarda, obtiene y borra la sesión/token; gestiona vencimiento de sesión.
- `auth/roles.js`: traduce roles del backend (`Student`, `AcademicSecretary`) a perfiles de navegación.
- `auth/permisos.js`: configuración del menú por perfil.
- `auth/useExpiracionSesion.js`: hook asociado al vencimiento de sesión.
- `router/withSuspense.jsx`: envoltorio para cargar módulos lazy con `Suspense`.
- `layouts/AppLayout.jsx`, `Header.jsx`, `Sidebar.jsx`: marco visual autenticado y navegación.
- `components/SimuladorRoles.jsx`: selector de perfil para desarrollo cuando está habilitado.
- `components/ui/ManagementTable.jsx`: tabla reutilizable para vistas de gestión.

#### `src/modules/`: funcionalidades organizadas por perfil

- `auth/pages/`
  - `LoginPage.jsx`: formulario de inicio de sesión.
  - `AccesoDenegadoPage.jsx`: aviso cuando el perfil no puede acceder a una ruta.
- `secretario/`
  - `secretarioRoutes.jsx`: define las rutas del perfil y carga las pantallas de forma diferida.
  - `pages/Inicio/HomePage.jsx`: inicio/panel del secretario.
  - `pages/PlanesEstudio/PlanesEstudio.jsx` y `PlanCard.jsx`: búsqueda y tarjetas de planes; las acciones actuales trabajan con datos mock.
  - `pages/Docentes/Docentes.jsx` y `ManagementPage.jsx`: interfaz reutilizable de gestión de docentes.
  - `pages/Inscripciones/InscripcionesContainer.jsx` y `components/YearCard.jsx`: resumen de inscripciones por año.
  - `components/plans/plansApi.js`: repositorio en memoria con datos iniciales para planes.
  - `components/teachers/teachersApi.js`: repositorio en memoria con datos iniciales para docentes.
  - `components/enrollments/useEnrollments.js`: hook de datos de demostración para el resumen de inscripción.
- `estudiante/`
  - `estudianteRoutes.jsx`: rutas del perfil estudiante.
  - `pages/Inicio/HomePage.jsx`: panel del estudiante, actualmente alimentado por datos de ejemplo.
  - `pages/Inscripciones/InscripcionesEstudiante.jsx`: consulta/inscripción visual a materias con datos locales.
  - `pages/Inscripciones/components/MateriaInscripcionCard.jsx`: tarjeta reutilizable de materia.
  - `components/enrollments/useEnrollments.js`: datos locales de ejemplo para inscripciones.

#### `src/api/`, `src/hooks/` y `src/data/`

- `api/httpClient.js`: función `request`, construye solicitudes HTTP usando `VITE_API_BASE_URL`, adjunta el token y representa errores HTTP con `ApiError`.
- `api/authApi.js`: operación de login que llama a `/api/auth/login`.
- `api/createMockRepository.js`: CRUD asíncrono en memoria para prototipos; los cambios se pierden al recargar la página.
- `hooks/useAsyncResource.js`: estado común de carga/error y recarga para recursos asíncronos.
- `hooks/useCrud.js`: estado y acciones CRUD reutilizables para pantallas de gestión.
- `data/managementData.js`: configuración de columnas y datos/acciones para vistas de gestión.

#### Rutas del frontend

| Perfil | Ruta | Pantalla |
|---|---|---|
| Público | `/login` | Inicio de sesión |
| Secretario | `/secretario/inicio` | Panel del secretario |
| Secretario | `/secretario/planes-de-estudio` | Planes de estudio |
| Secretario | `/secretario/inscripciones` | Resumen de inscripciones |
| Secretario | `/secretario/docentes` | Docentes |
| Estudiante | `/estudiante/inicio` | Panel del estudiante |
| Estudiante | `/estudiante/inscripciones` | Inscripción a materias |

La aplicación redirige al inicio correspondiente al perfil. Las rutas desconocidas vuelven a ese inicio o al login. El simulador está habilitado solo en desarrollo y con `VITE_ENABLE_ROLE_SIMULATOR=true`.

### `backend/`: API .NET

La solución `AcademicSystem.sln` contiene cuatro proyectos dirigidos a `net10.0`:

1. **`AcademicSystem.Api`**: HTTP, controladores, autenticación/autorización, CORS y configuración.
2. **`AcademicSystem.Business`**: servicios y reglas de negocio.
3. **`AcademicSystem.Data`**: contexto Entity Framework Core y repositorios.
4. **`AcademicSystem.Entities`**: modelos de base y DTOs compartidos.

Cada proyecto contiene un `.csproj`, que declara su framework, dependencias NuGet y referencias a los otros proyectos. `dotnet-tools.json` fija la versión local de `dotnet-ef`; `backend/.gitignore` evita versionar artefactos de compilación de .NET.

#### Proyecto `AcademicSystem.Api`

- `Program.cs`: punto de composición. Lee la conexión SQL Server y la clave JWT desde configuración, registra contexto/servicios, configura JWT, CORS, controllers y OpenAPI. El fallback de autorización exige usuario autenticado salvo endpoints declarados anónimos.
- `Controllers/`: endpoints:
  - `AuthController`: `POST /api/auth/login` (anónimo).
  - `HealthController`: `GET /api/health` (anónimo; verifica disponibilidad de la base y devuelve estado/cantidad de roles).
  - `UsersController`: CRUD de usuarios; requiere rol `AcademicSecretary`.
  - `RolesController`: operaciones sobre roles; requiere rol `AcademicSecretary`.
  - `StudentsController`: operaciones sobre estudiantes; requiere rol `AcademicSecretary`.
  - `AcademicSecretariesController`: operaciones sobre secretarios académicos; requiere rol `AcademicSecretary`.
- `Security/JwtTokenService.cs` y `IJwtTokenService.cs`: creación de tokens y contrato del servicio.
- `DevelopmentSeeder.cs`: crea usuarios de prueba al iniciar en Development si aún no existen.
- `appsettings.json`: valores de desarrollo predeterminados para conexión, CORS y metadatos JWT. No guardar claves JWT ni reemplazar configuración individual local aquí.
- `appsettings.Development.json`: configuración de logging específica de desarrollo.
- `Properties/launchSettings.json`: perfiles locales; el perfil HTTP usa `http://localhost:5000`.
- `AcademicSystem.Api.http`: solicitud de ejemplo para probar `/api/health`.

#### Proyecto `AcademicSystem.Business`

`Services/` contiene contratos y lógica de aplicación: `IAuthService`/`AuthService`, `IHealthService`/`HealthService`, `IUserService`/`UserService` y `RoleNames`. Cada interfaz concreta que se inyecte debe registrarse en `Program.cs`.

#### Proyecto `AcademicSystem.Data`

- `Context/AcademicSystemContext.cs`: `DbContext` y mapeo de entidades/tablas.
- `Repositories/IUserRepository.cs` y `UserRepository.cs`: acceso a usuarios usado, entre otros lugares, por autenticación.

#### Proyecto `AcademicSystem.Entities`

- `Models/`: clases que representan tablas y relaciones: `AbsenceJustification`, `AcademicSecretary`, `AssessmentInstance`, `CourseRecord`, `Director`, `DocumentType`, `Exam`, `PartialGrade`, `Prerequisite`, `ProgramContent`, `Role`, `RoleDocumentType`, `Student`, `StudentSubject`, `StudyPlan`, `StudyPlanSubject`, `Subject`, `SubjectProgram`, `Teacher`, `TeacherSubject`, `User` y `UserDocument`.
- `DTOs/`: contratos de entrada/salida: `HealthResponseDto`, `LoginRequestDto`, `LoginResponseDto` y `UserResponseDto`. Preferir DTOs en endpoints para no exponer entidades internas.

Los modelos y el contexto son scaffolding desde el esquema SQL. Antes de modificarlos, coordinar el cambio del esquema/script y la regeneración; no editar manualmente un archivo generado sin acordar cómo mantenerlo.

### `database/`: esquema SQL

`AcademicSystem.sql` crea la base `AcademicSystem`, sus tablas, claves y datos base. El esquema actual incluye estas 22 tablas:

`Roles`, `Users`, `Directors`, `AcademicSecretaries`, `Teachers`, `Students`, `Subjects`, `TeacherSubjects`, `StudentSubjects`, `Exams`, `SubjectPrograms`, `ProgramContents`, `DocumentTypes`, `RoleDocumentTypes`, `UserDocuments`, `AbsenceJustifications`, `StudyPlans`, `StudyPlanSubjects`, `Prerequisites`, `AssessmentInstances`, `PartialGrades` y `CourseRecords`.

El script es la referencia para la forma de la base. Revisá restricciones y relaciones en el propio SQL antes de cambiar persistencia o regenerar las entidades.

### `documentos/`: especificación y operación del equipo

- `guia-del-proyecto.md`: esta guía técnica de orientación.
- `Sistema Académico.md`: requerimientos y alcance funcional.
- `Equipos Proyectos 2do.md`: equipos e integrantes.
- `continuacion.md`: convenciones y continuidad técnica del proyecto.
- `diagrama_uml_Sistema_Académico.md`: diagrama UML.
- `flujo-git.md`: trabajo con ramas, forks y resolución de conflictos.
- `integracion-y-releases.md`: proceso de integración/promoción.
- `configuracion-github.md`: configuración y uso del repositorio en GitHub.

### `.github/`: colaboración y automatización

- `workflows/ci.yml`: CI de frontend (instalación, lint informativo, tests y build) y backend (`dotnet restore` y `dotnet build`).
- `PULL_REQUEST_TEMPLATE.md`: plantilla general de Pull Request.
- `PULL_REQUEST_TEMPLATE/promocion-a-testing.md` y `promocion-a-production.md`: plantillas de promoción.
- `CODEOWNERS`: propietarios sugeridos para revisión de cambios.

## 4. Cómo se conectan las partes

### Inicio de sesión

1. El frontend llama a `login()` en `src/api/authApi.js`.
2. `request()` en `src/api/httpClient.js` envía `POST /api/auth/login` a la URL configurada y convierte respuestas no exitosas en errores.
3. `AuthController` valida la solicitud y llama a `AuthService`.
4. `AuthService` busca un usuario activo y verifica el hash de la contraseña; limita intentos fallidos.
5. `JwtTokenService` firma el token. La respuesta entrega perfil y token, no el hash.
6. El frontend guarda la sesión y convierte el rol de backend en el perfil de navegación. Las rutas del cliente se restringen por sesión y perfil.

La protección del frontend mejora la navegación, pero la API también debe autorizar cada endpoint: nunca confiar únicamente en ocultar una ruta del cliente.

### Lectura/escritura de datos

El flujo de referencia es:

```text
Pantalla React -> módulo API/repository -> request HTTP -> Controller
              -> Service (Business) -> Repository/DbContext (Data)
              -> Model/DTO (Entities) -> SQL Server
```

El backend existente no usa aún esta separación de forma idéntica en todos los endpoints: algunas operaciones de controllers consultan el `DbContext` directamente. Para una nueva funcionalidad, seguir la convención acordada por el equipo y evitar duplicar acceso a datos.

## 5. Preparación y ejecución local

### Requisitos

- Node.js 20 o superior y npm.
- .NET SDK 10.
- SQL Server (la configuración inicial apunta a `localhost\\SQLEXPRESS`) y SQL Server Management Studio.

### Preparar la base

1. Abrir `database/AcademicSystem.sql` en SQL Server Management Studio.
2. Conectarse al servidor local y ejecutar el script (F5).
3. Si el servidor no es `localhost\\SQLEXPRESS`, guardar una cadena local con User Secrets, sin cambiar el archivo versionado:

```powershell
cd backend
dotnet user-secrets set "ConnectionStrings:AcademicSystem" "Server=TU-SERVIDOR;Database=AcademicSystem;Trusted_Connection=True;TrustServerCertificate=True" --project AcademicSystem.Api
```

### Configurar JWT y frontend

Desde `backend/`, crear una clave de desarrollo local para firmar JWT:

```powershell
$bytes = New-Object byte[] 48
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($bytes)) --project AcademicSystem.Api
```

Crear `.env.local` en la raíz a partir de `.env.example` y configurar:

```dotenv
VITE_API_BASE_URL=http://localhost:5000
VITE_ENABLE_ROLE_SIMULATOR=false
```

`VITE_*` son valores incorporados al cliente: no poner secretos en ellos. Reiniciar Vite al cambiar el archivo de entorno.

### Ejecutar

Terminal 1, desde la raíz:

```powershell
npm install
npm run dev
```

Frontend: `http://localhost:5173`.

Terminal 2:

```powershell
cd backend
dotnet run --project AcademicSystem.Api
```

API: `http://localhost:5000`. Comprobar `http://localhost:5000/api/health`; con SQL Server disponible debe indicar `status: ok`.

En Development se intentan crear estos usuarios de demostración si no existen:

| Perfil | Correo | Contraseña |
|---|---|---|
| Secretario | `maria@uni.edu` | `admin` |
| Estudiante | `loko@uni.edu` | `1234` |

Son credenciales exclusivamente locales/de desarrollo. El seeder no configura usuarios de producción.

### Ver la interfaz sin API

Para probar pantallas de presentación en desarrollo:

1. Poner `VITE_ENABLE_ROLE_SIMULATOR=true` en `.env.local`.
2. Reiniciar `npm run dev`.

El simulador solo funciona con Vite en desarrollo. No sustituye autenticación real ni hace persistentes los datos mock.

## 6. Comandos de validación

Desde la raíz del repositorio:

```powershell
npm run lint
npm test
npm run build
```

Para compilar el backend desde la raíz:

```powershell
dotnet restore backend/AcademicSystem.sln
dotnet build backend/AcademicSystem.sln --configuration Release --no-restore
```

Vitest busca pruebas bajo `src/**/*.test.js` y `src/**/*.test.jsx`. El workflow de CI permite que no haya pruebas todavía (`--passWithNoTests`); cuando agregues tests, incluí los casos relacionados con el cambio. El lint de CI es informativo, pero debe revisarse; el build de frontend y backend forma parte de CI.

## 7. Cómo implementar una funcionalidad

### Frontend

1. Confirmar el equipo/perfil y el módulo propietario del cambio.
2. Crear o actualizar la pantalla dentro de `src/modules/<perfil>/pages/`.
3. Agregar su ruta en `secretarioRoutes.jsx` o `estudianteRoutes.jsx` y, si corresponde, su entrada de menú en `src/core/auth/permisos.js`.
4. Mantener la lógica de solicitudes en `src/api/` o en el API del módulo; evitar fetch directo disperso por componentes.
5. Reutilizar los hooks y componentes existentes si su contrato coincide.
6. Agregar o actualizar estilos bajo `src/styles/` siguiendo el patrón del módulo.
7. Mientras el endpoint no exista, identificar explícitamente los datos como mock. Al migrar a API real, cubrir estados de carga, error y vacío.

### Backend

1. Definir el contrato de entrada/salida (DTO) y reglas de validación.
2. Implementar el endpoint en un controller de `AcademicSystem.Api`.
3. Colocar las reglas de negocio en `AcademicSystem.Business/Services` y persistencia en `AcademicSystem.Data` según el patrón acordado.
4. Proteger el endpoint con autorización; `Program.cs` aplica autorización autenticada por defecto.
5. Registrar servicios nuevos en `Program.cs`, actualizar pruebas/documentación y compilar la solución.
6. Si cambia el esquema, coordinar el SQL, revisar claves/restricciones y actualizar el scaffolding de `Entities/Models` y `Data/Context`.

## 8. Reglas de colaboración importantes

- Trabajar en la rama del equipo y subir al fork personal mediante Pull Request; no pushear directo al repositorio principal.
- Antes de comenzar, traer los cambios integrados de `development`.
- No subir `node_modules/`, `dist/`, `bin/`, `obj/`, `.env.local` ni claves.
- No editar `appsettings.json` para personalizar la conexión local: usar User Secrets.
- No usar `git push --force`.
- Los archivos compartidos (`src/core/`, configuración, `Program.cs`) deben recibir cambios puntuales y coordinados.
- Antes del PR, revisar `git status`, ejecutar los validadores pertinentes y usar la plantilla correspondiente.

La guía completa del flujo está en [flujo-git.md](./flujo-git.md); la incorporación y resolución de problemas comunes se explica en el [README del repositorio](../README.md).
