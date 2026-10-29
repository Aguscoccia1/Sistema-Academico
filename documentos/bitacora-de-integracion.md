# Bitácora de integración

Registro de todo lo que entró a `development`: qué trajo cada equipo, cómo se verificó y qué quedó pendiente. La mantiene el E1 (Acceso e Integración).

## Cómo se integra

1. Cada equipo trabaja en su fork, en su rama (`e1`, `e2`, `e3` o `e4`), y abre un Pull Request hacia `development`.
2. El E1 revisa cada PR antes de mergearlo:
   - compila el frontend (`npm run build` y `npm run lint`) y el backend (`dotnet build`);
   - prueba la API contra una base nueva creada con `database/AcademicSystem.sql`, con el usuario Secretario y con el Estudiante;
   - prueba las pantallas en el navegador;
   - combina las ramas de todos los equipos para encontrar conflictos antes de que lleguen a `development`.
3. Lo que funciona se integra. Si algo falla, se le informa al equipo para que lo corrija en su rama: el E1 no corrige el código de otros equipos.

## Estado al 29/10/2026

Los cuatro equipos ya tienen su trabajo integrado en `development`.

| Equipo | Qué está integrado | Qué falta |
|---|---|---|
| **E1** — Acceso e Integración | Login, cierre y vencimiento de sesión (con renovación automática), roles y permisos, menú por rol, API y pantalla de gestión de usuarios. | Recuperación de contraseña (no hay servidor SMTP). |
| **E2** — Secretaría | Inscripción a 1.º año, ver y modificar estudiantes, comisiones con asignación automática, gestión de cupos, docentes con asignación de materias y horarios por comisión, con las pantallas conectadas a la API. | Hacer el resumen de inscripciones con menos pedidos a la API. |
| **E3** — Gestión Académica | API y pantallas de planes de estudio y materias, pantalla de correlatividades. | Guardar las correlatividades en la base, bajas y cursos. |
| **E4** — Inscripciones | Materias disponibles, validación de correlativas e inscripción con control de duplicados (API); pantallas conectadas a la API. | Que la lista de materias informe la comisión y que "Mis inscripciones" lea la base. |

## Línea de tiempo

| Fecha | Qué pasó |
|---|---|
| 31/08 | Minuta 1: alcance del E1. |
| 03/09 | Primer commit del repositorio. |
| 14/09 | Ramas por equipo y CI. |
| 21/09 | Arranca el Sprint 1. |
| 24/09 | El repositorio se alinea con la base de la profesora. |
| 26/09 | Script de la base de datos `AcademicSystem` (22 tablas), armado desde el UML. |
| 28/09 | Backend en capas (Api, Business, Data, Entities). PR #1 a `production`. |
| 06/10 | Guía para trabajar con forks. |
| 11/10 | El E1 abre el PR #2. Matriz de roles y permisos publicada como borrador. |
| 14/10 | Primeros reportes de seguimiento. Integración del E1 (PR #2 dentro del #6). |
| 15/10 | Integración del E3 (PR #4). Demo funcional. |
| 19/10 | Integración del E4 (PR #5). Carpeta `src/services`. Se sacan los datos simulados. |
| 28/10 | Integración del E1 (pantalla de usuarios y renovación de sesión), del E3 (PR #8) y del E4 (PR #5). Revisión del E2 con guía de correcciones. |
| 29/10 | El E2 aplica las correcciones y se integra. Los cuatro equipos quedan en `development`. |

## Integraciones

### 28/09 — Base del proyecto (PR #1)

- Estructura de la profesora, ramas `e1`–`e4` → `development` → `testing` → `production`, CI y README.
- Base de datos `AcademicSystem` y backend en capas.

### 14/10 — Acceso del E1 (PR #2, mergeado dentro del #6)

- **Qué entró:** login con correo y contraseña (token de 30 minutos con el id y el rol), bloqueo al sexto intento fallido, rechazo de usuarios dados de baja, cierre de sesión, vencimiento por inactividad, 401 y 403 por rol, pantalla de acceso denegado, menú por rol y API de usuarios.
- **Correcciones del integrador antes de mergear:** se sacó `database/.vs/`, se renombró `permisos,js` a `permisos.js` y el login pasó a decir "Correo".
- **Verificación:** 28 pruebas de API y 12 en el navegador, todas OK.

### 15/10 — Planes de estudio del E3 (PR #4)

- **Qué entró:** API de planes de estudio y materias, con validaciones, y las pantallas de planes.
- **Durante la integración:** se detectó que un estudiante logueado podía crear planes y editar materias. El E3 agregó el control de rol (`[Authorize(Roles = RoleNames.AcademicSecretary)]`) en la misma reunión.
- **Verificación:** 24 pruebas de la API y 20 de 20 en las pruebas de roles.

### 19/10 — Inscripciones del E4 y orden del frontend

- **Qué entró del E4 (PR #5, primera parte):** pantalla de inscripción, "Mis inscripciones" y los endpoints de materias disponibles y validación de correlativas.
- **Choque resuelto:** E3 y E4 habían creado `SubjectController` y los mismos DTOs. Quedó el E3 como dueño de materias y planes, y el E4 pasó a `EnrollmentController`.
- **Pedido de la profesora:**
  - las llamadas a la API se ordenaron en `src/services/`, con un archivo por cada controller; los services de un solo model van en su carpeta (`estudiante/`, `secretario/`);
  - se sacaron todos los datos simulados, el simulador de roles y la pantalla de prueba de Docentes. Las pantallas sin conectar aparecen vacías.
- **Verificación:** login 12 de 12, roles 20 de 20 y las pantallas sin errores en la consola. El lint quedó en 0 errores.

### 28/10 — E1, E3 y E4

- **E1 (fork de Damián, rama `e1`):**
  - pantalla de gestión de usuarios con alta, edición y baja (S2-37);
  - la lógica de usuarios pasó a la capa de servicio;
  - la sesión se renueva sola con un refresh token mientras haya actividad: vence a los 30 minutos sin actividad, a las 12 horas, o a los 7 días con "Recordarme";
  - nuevos usuarios de prueba: `fernando@uni.edu` / `admin` y `esteban@uni.edu` / `1234`.
- **E3 (PR #8):** pantalla de correlatividades con confirmación, el detalle del plan guarda y lee las materias en la base, y el hook `useSubmit`.
- **E4 (PR #5):**
  - inscripción con validación de correlativas y control de duplicados;
  - endpoint "mis inscripciones" (`GET /api/enrollments/me`);
  - el plan activo en las materias disponibles;
  - las pantallas conectadas a la API.
- **Verificación:**
  - las tres ramas se combinan sin conflictos y compilan;
  - API: 25 de 25 pruebas (login, renovación de sesión, usuarios, planes, materias, materias disponibles, correlativas, inscripción y duplicados, con los dos roles);
  - navegador: login, pantalla de usuarios, renovación de sesión con el token vencido, planes y la lista de materias del estudiante, sin errores en la consola;
  - lo que falló: la inscripción desde la pantalla del E4, por el problema de la comisión que está en los pendientes.
- **No entró:** el E2. Tenía `Server=ALAN` en la configuración, había reescrito el `httpClient` (eso pisaba la renovación de sesión), tenía controllers sin control de rol, models editados a mano, datos de prueba en el script y archivos de datos simulados. Se le entregó una guía de correcciones paso a paso.

### 29/10 — Secretaría del E2 (fork de Alan, rama `e2`)

- **Qué entró:**
  - inscripción de estudiantes a 1.º año y cantidad de inscriptos (`/api/inscripciones`);
  - ver y modificar estudiantes, y reasignarles la comisión (`/api/estudiantes`);
  - comisiones por materia y asignación automática de estudiantes a comisiones (`/api/comisiones`);
  - docentes con asignación de materias y comisiones (`/api/teachers`);
  - horarios por materia y comisión, con control de superposición (`/api/horarios-materias`) y la tabla nueva `SubjectSchedules`;
  - las pantallas Inscripciones, Gestionar cupos, Visualizar y Modificar estudiante, Docentes, y Visualizar y Modificar docente, todas conectadas a la API.
- **Correcciones que aplicó el equipo (las 10 de la guía):**
  - volvió la conexión a `localhost\SQLEXPRESS`;
  - dejó el `httpClient`, el header y `vite.config.js` como en `development`;
  - agregó el control de rol en sus controllers;
  - puso los nombres de archivo con mayúscula;
  - sacó los datos de prueba del script;
  - borró `Registration.cs` y los archivos de datos simulados;
  - ordenó sus services, uno por controller;
  - restauró la guía de Git;
  - en `Program.cs` agregó solo sus líneas.
- **Verificación:**
  - frontend y backend compilan; el lint no suma errores;
  - el script de la base se corre dos veces seguidas sin errores;
  - API: 24 pruebas del E2 OK, con los dos roles y sin sesión, y la regresión de E1, E3 y E4 sigue en 25 de 25;
  - navegador: 13 de 13. Las 7 pantallas del E2 andan sin errores en la consola, Usuarios y Planes siguen funcionando, el estudiante no entra a las pantallas del Secretario y la inscripción del estudiante sigue andando.
- **Cómo entró:** no abrió Pull Request. Se integró mergeando su rama del fork, igual que la del E1.

## Decisiones tomadas

| Tema | Decisión |
|---|---|
| Flujo de trabajo | Cada integrante usa su fork y abre un PR hacia `development`. Solo el E1 mergea. |
| Backend | Capas Api, Business, Data (con el contexto en `Data/Context`) y Entities (`DTOs` y `Models`). Los models se generan desde la base y no se editan a mano. |
| Conexión a la base | `appsettings.json` apunta a `localhost\SQLEXPRESS`. Cada uno guarda su conexión con `dotnet user-secrets`. |
| Seguridad | Todo endpoint pide sesión y cada controller declara su rol con `[Authorize(Roles = ...)]`. |
| Frontend | Las llamadas a la API van en `src/services/`, un archivo por controller. No hay datos simulados en las pantallas. |
| Recuperación de contraseña | Se posterga: no hay servidor SMTP. Mientras tanto, el secretario puede asignar una contraseña nueva editando al usuario. |
| Horarios de materias | Los consulta cualquier usuario con sesión (el estudiante también los necesita). Solo el Secretario los carga. |

## Pendientes por equipo (al 29/10/2026)

### E1

- Recuperación de contraseña, cuando haya SMTP.
- Actualizar el paquete `System.Security.Cryptography.Pkcs` 9.0.11 (12 avisos de vulnerabilidad al compilar).
- Promover `development` a `testing` y después a `production`: `production` sigue en la versión del 28/09.

### E2

- El resumen de inscripciones (`useEnrollments`) hace un pedido a la API por cada estudiante: con muchos alumnos va a ser lento. Conviene un endpoint que devuelva el resumen armado.
- `GET /api/teachers` lista solo los docentes que ya tienen materias asignadas. La pantalla de Docentes arma la lista desde los usuarios, así que no falta nadie, pero conviene que el endpoint devuelva todos.
- Próximas entregas: que trabajen desde su fork los otros integrantes, porque todos los commits son de Alan, y abrir Pull Request en lugar de pasar solo la rama.

### E3

- Las correlatividades solo se guardan en la pantalla: al recargar se pierden, aunque el aviso dice "se guardaron correctamente". Falta el endpoint para guardarlas en la tabla `Prerequisites`, que es la que usa el E4 para validar.
- `useSubmit` no vuelve `isSubmitting` a `false` cuando la operación sale bien.
- Bajas de planes y materias, y cursos.

### E4

- `GET /api/enrollments/available-subjects` no devuelve la comisión (`section`), y la pantalla la necesita para inscribir. Hoy, desde la pantalla, la inscripción no se puede hacer.
- "Mis inscripciones" usa lo que quedó en la pantalla y no el endpoint `GET /api/enrollments/me`. Al recargar no muestra lo guardado.
- `InscripcionesEstudiante.jsx` usa `PERIODO_DE_INSCRIPCION` sin definirla. Es el único error de lint del proyecto y rompería la pantalla si el período se marcara como cerrado.
- Pasaron `enrollmentService` y `prerequisiteValidationService` a `src/services/estudiante/`. La regla acordada es que esa carpeta tiene solo el service del model Student: hay que decidir si se acepta o vuelven a la raíz de `services/`.

### Mesa Técnica

- Hay dos controllers de estudiantes: el del E1 (`StudentsController`, `/api/students`) y el del E2 (`StudentController`, `/api/estudiantes`). Hay que decidir cuál queda para cada cosa.
- Aprobar formalmente la tabla `SubjectSchedules` (horarios por materia y comisión), que entró con el E2.

## Números del proyecto (al 29/10/2026)

En `development` hay 104 commits sin contar los merges:

| Integrante | Equipo | Commits |
|---|---|---|
| Alan Ordóñez | E2 | 17 |
| Tomás González | E1 | 17 |
| Lorenzo Caro | E3 | 13 |
| Joaquín Olmedo | E1 | 12 |
| Agustín Coccia | E4 | 9 |
| Damián Palacios | E1 | 7 |
| Carlos Luque Heredia | E1 | 7 |
| Federico Petta | E3 | 6 |
| Esteban Soria | E4 | 5 |
| Facundo Pesa Giménez | E4 | 5 |
| Franco Petta | E1 | 4 |
| Jeremías Herrera | E4 | 2 |

En GitHub solo aparecen commits de 12 de los 19 integrantes. El trabajo de quienes no subieron nada desde su fork no se ve acá.
