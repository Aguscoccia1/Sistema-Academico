# Guía de Continuidad del Sistema Académico

## 1. Estado actual del proyecto

El proyecto tiene un frontend en React + Vite y un backend en .NET con SQL Server. Hay dos perfiles: secretario y estudiante. Para entrar hay que iniciar sesión, y cada perfil ve solo lo suyo. Ya no hay datos simulados: toda la información viene de la API.

Actualmente la aplicación contempla:

- Inicio de sesión, cierre y vencimiento de sesión, y permisos por rol.
- Gestión de planes de estudio y materias, conectada a la API.
- Inscripciones del secretario, con la pantalla lista para conectar a la API.
- Vista del estudiante con la inscripción a materias y sus inscripciones, también para conectar a la API.

El acceso a datos está separado de las pantallas: en `src/services/` hay un archivo por cada controller del backend, y `httpClient.js` arma cada pedido.

## 2. Estructura real del frontend

```text
Sistema Académico/
├── index.html
├── package.json
├── vite.config.js
├── vitest.config.js
├── eslint.config.js
├── backend/                → API en .NET (Api, Business, Data, Entities)
├── database/               → script de la base de datos
├── documentos/
├── src/
│   ├── main.jsx
│   ├── services/
│   │   ├── httpClient.js   → arma cada pedido: dirección de la API, token y sesión vencida
│   │   ├── <nombre>Service.js   → uno por cada controller del backend
│   │   ├── estudiante/     → studentsService.js
│   │   └── secretario/     → academicSecretariesService.js
│   ├── assets/
│   ├── core/
│   │   ├── auth/           → sesión, roles, permisos y vencimiento por inactividad
│   │   ├── layouts/        → AppLayout, Header y Sidebar
│   │   ├── router/         → withSuspense.jsx
│   │   └── routes/         → App.jsx
│   ├── hooks/
│   │   └── useAsyncResource.js
│   ├── modules/
│   │   ├── auth/
│   │   │   └── pages/      → LoginPage y AccesoDenegadoPage
│   │   ├── estudiante/
│   │   │   ├── estudianteRoutes.jsx
│   │   │   └── pages/
│   │   │       ├── Inicio/
│   │   │       └── Inscripciones/
│   │   └── secretario/
│   │       ├── secretarioRoutes.jsx
│   │       ├── components/
│   │       │   └── plans/plansApi.js
│   │       └── pages/
│   │           ├── Inicio/
│   │           ├── Inscripciones/
│   │           └── PlanesEstudio/
│   └── styles/
└── node_modules/
```

> Los directorios `dist/` y `node_modules/` se generan automáticamente y no deben versionarse ni editarse manualmente.

## 3. Patrón arquitectónico actual

La aplicación usa una separación funcional por roles y por módulos, con un shell global compartido:

- `src/core/routes/App.jsx`: punto central de ruteo; protege las rutas por sesión y por rol.
- `src/modules/secretario/secretarioRoutes.jsx`: rutas del rol secretario.
- `src/modules/estudiante/estudianteRoutes.jsx`: rutas del rol estudiante.
- `src/core/layouts/AppLayout.jsx`: layout base del dashboard con sidebar y header.
- `src/modules/.../pages`: pantallas específicas del dominio.
- `src/services/`: un archivo por cada controller del backend; los de un solo model van en su carpeta (`estudiante/`, `secretario/`).
- `src/services/httpClient.js`: función `request` que usan todos los services.

La navegación no se define en un único archivo global de páginas, sino en archivos de rutas asociados a cada módulo. El rol sale de la sesión iniciada. Ese patrón debe mantenerse como base para continuar creciendo.

## 4. Cómo se integra la capa de datos

Cada service llama a los endpoints de un controller con `request`:

```js
// src/services/studyPlanService.js
import { request } from './httpClient';

export const studyPlanService = {
  obtenerTodos: () => request('/api/StudyPlan'),
  crear: (plan) => request('/api/StudyPlan', { method: 'POST', body: JSON.stringify(plan) }),
};
```

Y la pantalla usa el service, nunca `fetch` directo:

```js
const { data: planes, isLoading, error } = useAsyncResource(studyPlanService.obtenerTodos, []);
```

Si una pantalla necesita adaptar los datos entre el formato del backend y el de la vista, lo hace en un archivo propio del módulo, como `plansApi.js`, que a su vez usa los services.

## 5. Arquitectura recomendada para continuar

El proyecto debe seguir una lógica incremental basada en la estructura ya existente:

1. Mantener `src/core` para infraestructura compartida: layout, rutas, sesión y componentes globales.
2. Mantener `src/modules` como eje principal de feature/rol: secretario y estudiante.
3. Crear componentes y hooks internos por cada módulo antes de duplicar lógica entre pantallas.
4. Evitar mezclar acceso a datos, validaciones y render dentro de un mismo componente cuando crezca la complejidad.
5. Usar `id` como clave principal en todos los modelos y operaciones de actualización/eliminación.
6. Toda llamada a la API pasa por su service en `src/services/`. Si un equipo crea un controller nuevo, crea también su service.

## 6. Contratos con el backend .NET

Los contratos se definen primero con la Mesa Técnica. Cada controller del backend tiene su service en `src/services/`:

| Controller | Ruta | Service |
|---|---|---|
| `AuthController` | `POST /api/auth/login` | `authService.js` |
| `PermissionsController` | `GET /api/auth/permissions` | `permissionsService.js` |
| `StudyPlanController` | `/api/StudyPlan` | `studyPlanService.js` |
| `SubjectController` | `/api/Subject` | `subjectService.js` |
| `UsersController` | `/api/users` | `usersService.js` |
| `StudentsController` | `/api/students` | `estudiante/studentsService.js` |
| `AcademicSecretariesController` | `/api/academic-secretaries` | `secretario/academicSecretariesService.js` |
| `RolesController` | `/api/roles` | `rolesService.js` |
| `EnrollmentController` | `/api/enrollments` | `enrollmentService.js` |
| `HealthController` | `/api/health` | `healthService.js` |
| `PrerequisiteValidationController` | `/api/prerequisites` | `prerequisiteValidationService.js` |

Se recomienda documentar DTOs, códigos HTTP, errores normalizados, validaciones y autenticación antes de implementar cada endpoint nuevo.

## 7. Convenciones de código del proyecto

### Nombres y organización

- Componentes React: `PascalCase`.
- Archivos JSX: `PascalCase`.
- Hooks: prefijo `use`, por ejemplo `useAsyncResource`.
- Funciones y variables: `camelCase`.
- Constantes globales: `UPPER_SNAKE_CASE` cuando corresponda.
- Identificadores de entidad: `id` y nombres descriptivos como `planId`, `materiaId`, `estudianteId`.

### React y estado

- Mantener el estado inmutable.
- Usar actualizaciones funcionales cuando el siguiente estado dependa del anterior.
- No mutar arrays ni objetos directamente.
- Tratar la lista como fuente de verdad y actualizar desde callbacks del hook o del servicio.
- Manejar estados `loading`, `error` y `empty` en operaciones asíncronas.
- Evitar efectos innecesarios o respuestas pendientes sin limpieza.

### Estilos y presentación

- Los estilos se encuentran en `src/styles/` y deben usarse como base visual compartida.
- La estructura de estilos actual puede continuar evolucionando, pero siempre conviene separar layout, navegación, tablas y módulos específicos.
- No mezclar lógica de negocio con presentación; los componentes deben delegar la lógica a hooks o servicios.

## 8. Continuidad del desarrollo

La continuidad del proyecto debe enfocarse en estos puntos prioritarios:

1. Consolidar la base de rutas y roles con `secretario` y `estudiante`.
2. Completar el dominio de planes, docentes y estudiantes con formularios y validaciones.
3. Conectar a la API las pantallas que todavía no muestran datos, con sus estados de carga, error y vacío.
4. Crear hooks específicos por entidad para centralizar carga, edición y eliminación.
5. Definir DTOs y contratos en la Mesa Técnica antes de crear cada endpoint nuevo.
6. Ejecutar validaciones finales antes de cada entrega:

```bash
npm run lint
npm test
npm run build
```

## 9. Recomendaciones finales

La base actual del proyecto es viable y estructurada, pero aún requiere mayor definición de dominio y de contratos. La estrategia correcta es continuar desde la arquitectura actual de módulos, no reescribir la solución completa. El proyecto debe evolucionar de forma incremental, manteniendo la separación entre UI, rutas, hooks y acceso a datos, y dejando la capa de backend desacoplada para facilitar la integración con .NET.

Este enfoque preserva la continuidad del proyecto, reduce el riesgo de cambios masivos y permite que cada equipo o integrante trabaje por módulos sin romper el flujo principal del sistema.
