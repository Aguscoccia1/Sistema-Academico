# Bitácora de integración

Registro de todo lo que entró a `development`: qué trajo cada equipo, cómo se verificó y qué quedó pendiente. La mantiene el E1 (Acceso e Integración).

## Cómo se integra

1. Cada equipo trabaja en su fork, en su rama (`e1`, `e2`, `e3` o `e4`), y abre un Pull Request hacia `development`.
2. El E1 revisa cada PR antes de mergearlo:
   - compila el frontend (`npm run build` y `npm run lint`) y el backend (`dotnet build`);
   - comprueba que el backend respete las capas: controller, service, repository y base;
   - prueba la API contra una base nueva creada con `database/AcademicSystem.sql`, con el usuario Secretario y con el Estudiante;
   - prueba las pantallas en el navegador;
   - combina las ramas de todos los equipos para encontrar conflictos antes de que lleguen a `development`.
3. Lo que funciona se integra. Si algo falla, se le informa al equipo para que lo corrija en su rama: el E1 no corrige el código de otros equipos.

## Estado al 13/11/2026

Los cuatro equipos ya tienen su trabajo integrado en `development`.

| Equipo | Qué está integrado | Qué falta |
|---|---|---|
| **E1** — Acceso e Integración | Login, cierre y vencimiento de sesión (con renovación automática), roles y permisos, menú por rol, API y pantalla de gestión de usuarios. | Auditoría de accesos (S2-9), menú según los permisos de la API (S2-10) y recuperación de contraseña (S2-7, sin servidor SMTP). |
| **E2** — Secretaría | Inscripción a 1.º año, ver y modificar estudiantes, comisiones con asignación automática, gestión de cupos, docentes con asignación de materias y horarios por comisión, con las pantallas conectadas a la API. | Hacer el resumen de inscripciones con menos pedidos a la API. |
| **E3** — Gestión Académica | API y pantallas de planes de estudio y materias, correlatividades guardadas en la base con sus validaciones, y quitar materias de un plan. | Bajas de planes, cursos y el arreglo de `useSubmit`. |
| **E4** — Inscripciones | Materias disponibles, validación de correlativas, inscripción desde la pantalla y "Mis inscripciones" leyendo la base. | Definir el período de inscripción y ofrecer solo materias de 2.º y 3.º año. |

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
| 08/11 | Integración del E3 y el E4 con sus pases a repositorios. El E1 pasa a las cuatro capas. Se define la regla controller, service, repository y base. |
| 13/11 | Se integran las correlatividades del E3 (PR #9) y el repositorio de validación del E4 (PR #10). El pase a capas del E2 queda en revisión. |
| 13/11 | El E2 corrige la asignación automática y se integra su pase a capas (PR #11). Todo el backend cumple la regla de las cuatro capas. |

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

### 08/11 — Repositorios del E3 y el E4, y capas del E1

- **Regla nueva:** toda request recorre controller, service, repository y base. El README decía que el service usaba el contexto y el ejemplo `HealthService` iba directo a la base: se corrigieron los dos.
- **E3 (rama `e3`, sin Pull Request):** `StudyPlan` y `Subject` pasaron a repositorios.
- **E4 (rama `e4`, sin Pull Request):**
  - `Enrollment` pasó a repositorio;
  - el backend asigna la comisión al inscribir, así que inscribirse desde la pantalla guarda en la base;
  - "Mis inscripciones" lee la base y se mantiene al recargar.
- **E1:**
  - `StudentsController`, `AcademicSecretariesController` y `RolesController` dejaron de usar la base: su lógica pasó a services y sus consultas a repositorios;
  - `UserService` y `HealthService` pasaron a repositorios.
- **Error corregido en el E1:** crear o editar un perfil de estudiante o de secretario siempre devolvía 400, porque esos controllers recibían el model de la base. Ahora reciben DTOs y responden con sus validaciones.
- **Verificación:**
  - frontend y backend compilan, y el lint quedó en 0 errores;
  - comparación de la API del E1 antes y después con 76 pedidos: solo cambiaron los 27 de crear, editar y borrar perfiles, que antes daban 400; usuarios, roles, login y health respondieron igual;
  - regresión de API: E1, E3 y E4 25 de 25; E2 24 de 26, y las dos restantes son comportamiento esperado (los horarios los consulta cualquier usuario con sesión y `GET /api/teachers` solo lista docentes con materias);
  - navegador: 13 de 13 en las pantallas de los cuatro equipos, e inscripción de punta a punta desde la pantalla del estudiante.
- **Conflicto resuelto:** `Program.cs`, donde E3 y E4 agregaban líneas en el mismo lugar.
- **Dato para las demos:** una materia solo aparece disponible para el estudiante si tiene docente asignado con comisión. Primero hay que asignar los docentes.

### 13/11 — Correlatividades del E3 (PR #9) y validación del E4 (PR #10)

- **E4 (PR #10):** `PrerequisiteValidationService` dejó de usar la base directo: sus consultas pasaron a `PrerequisiteValidationRepository`. Las respuestas no cambiaron.
- **E3 (PR #9):**
  - las correlatividades se guardan en la tabla `Prerequisites`, con controller, service y repository;
  - se rechazan los ciclos, las repetidas, las correlativas de otro plan y la materia como correlativa de sí misma;
  - la pantalla carga las correlativas guardadas y, al guardar, manda todas las materias que cambiaron;
  - se puede quitar una materia de un plan, salvo que tenga evaluaciones; al quitarla se borran sus correlativas.
- **Resultado:** el circuito de correlativas quedó completo. La secretaría las carga desde la pantalla y el estudiante ve bloqueada la materia, con el nombre de la que le falta. Ya no hace falta cargarlas por SQL.
- **Corrección pedida al E3 antes de integrar (09/11):** al guardar solo se guardaba la materia seleccionada, y lo marcado en las otras se perdía. Lo corrigieron en el mismo PR.
- **Verificación**, sobre bases nuevas:
  - frontend y backend compilan, y el lint quedó en 0 errores;
  - correlativas, validación y quitar materias, por API y por pantalla: 49 de 49;
  - flujo de inscripción desde la pantalla del estudiante: 8 de 8;
  - la API del E1 respondió igual que antes en 78 pedidos, y el bloqueo de cuenta por intentos fallidos sigue funcionando;
  - regresión de API: E1, E3 y E4 25 de 25; E2 24 de 26, con las mismas dos diferencias esperadas del 08/11;
  - pantallas de los cuatro equipos: mismo resultado antes y después de integrar.
- **E2 (rama `e2`, sin Pull Request): no se integró.** El pase a las cuatro capas está hecho y cumple la regla. Pero en el mismo cambio la asignación automática de comisiones pasó a inscribir a todos los estudiantes del sistema en cada materia con horarios en A y B, de cualquier año y sin validar correlativas. En la prueba, un estudiante quedó inscripto en una materia de 2.º año con la correlativa sin aprobar. Lo corrigieron y se integró el mismo día, con el PR #11.

### 13/11 — Capas del E2 (PR #11)

- **E2 (PR #11):**
  - `TeachersController`, `InscripcionesService`, `StudentService` y `SubjectScheduleService` dejaron de usar la base directo: sus consultas pasaron a repositorios;
  - las reglas de comisiones y de asignación de docentes pasaron de los repositorios a los services;
  - la asignación automática de comisiones trabaja solo con materias de 1.º año y no crea inscripciones: empareja entre A y B a los estudiantes ya inscriptos;
  - cambios de diseño en las pantallas de inscripciones, estudiantes y docentes.
- **Resultado:** ningún controller ni service usa la base directo. Todo el backend recorre controller, service, repository y base.
- **Corrección pedida al E2 antes de integrar:** en la primera entrega, la asignación automática inscribía a todos los estudiantes del sistema en cada materia, de cualquier año y sin validar correlativas, y traía una copia de dos archivos del E4. Lo corrigieron en el mismo PR.
- **Ajuste del E1 al integrar:** `appsettings.json` llegó con el servidor de la máquina de Alan (`Server=ALAN`). Se volvió a `localhost`, porque con ese valor el backend de los demás no conecta. Las conexiones personales van con user-secrets.
- **Cambio en la API:** el pedido de la asignación automática pasó de `{ materiaId }` a `{ materiaIds: [...] }`, y exige horarios en las comisiones A y B.
- **Base del PR:** se abrió contra `production` en vez de `development`. Se integró en `development`, como los demás.
- **Verificación**, sobre bases nuevas:
  - frontend y backend compilan, y el lint quedó en 0 errores;
  - asignación automática: 18 de 18. Rechaza materias de 2.º año, no crea inscripciones, deja el reparto parejo y repetirla no cambia nada;
  - correlativas, validación y quitar materias: 49 de 49; flujo de inscripción del estudiante: 8 de 8;
  - la API del E1 respondió igual que antes en 78 pedidos, y el bloqueo de cuenta por intentos fallidos sigue funcionando;
  - regresión de API: E1, E3 y E4 25 de 25; E2 24 de 26, con las mismas dos diferencias esperadas del 08/11;
  - pantallas de los cuatro equipos: mismo resultado antes y después de integrar.

## Decisiones tomadas

| Tema | Decisión |
|---|---|
| Flujo de trabajo | Cada integrante usa su fork y abre un PR hacia `development`. Solo el E1 mergea. |
| Backend | Capas Api, Business, Data (con el contexto en `Data/Context` y los repositorios en `Data/Repositories`) y Entities (`DTOs` y `Models`). Los models se generan desde la base y no se editan a mano. |
| Flujo de una request | Controller, service, repository y base. Las reglas de negocio van en el service. Solo los repositorios usan el contexto. |
| Conexión a la base | `appsettings.json` apunta a `localhost\SQLEXPRESS`. Cada uno guarda su conexión con `dotnet user-secrets`. |
| Seguridad | Todo endpoint pide sesión y cada controller declara su rol con `[Authorize(Roles = ...)]`. |
| Frontend | Las llamadas a la API van en `src/services/`, un archivo por controller. No hay datos simulados en las pantallas. |
| Recuperación de contraseña | Se posterga: no hay servidor SMTP. Mientras tanto, el secretario puede asignar una contraseña nueva editando al usuario. |
| Horarios de materias | Los consulta cualquier usuario con sesión (el estudiante también los necesita). Solo el Secretario los carga. |

## Pendientes por equipo (al 13/11/2026)

### E1

- Auditoría de accesos (S2-9): todavía no hay código.
- Menú según los permisos de la API (S2-10): el menú sigue siendo una lista fija en `permisos.js`.
- Recuperación de contraseña (S2-7), cuando haya SMTP.
- Las pantallas de recuperación de contraseña y de consulta de auditoría están en el fork `0lypus`, con datos simulados y sin Pull Request: se integran cuando tengan backend.
- Actualizar el paquete `System.Security.Cryptography.Pkcs` 9.0.11, que tiene vulnerabilidades conocidas.
- Promover `development` a `testing` y a `production`: las dos siguen en la versión del 06/10, sin el trabajo de los equipos.
- Subir la versión aprobada al GitLab del instituto para el deploy: `https://git.icgb.com.ar/2do2026/gestion-academica`.

### E2

- El resumen de inscripciones hace un pedido a la API por cada estudiante; conviene un endpoint que lo devuelva armado.
- Los Pull Requests van contra `development`: el #11 se abrió contra `production`.
- Es la segunda vez que llega `Server=ALAN` en `appsettings.json`: la conexión personal va con user-secrets, como explica el README.
- Todos los commits son de Alan: faltan los aportes del resto del equipo.

### E3

- `useSubmit` no vuelve `isSubmitting` a `false` cuando la operación sale bien.
- Bajas de planes y cursos.

### E4

- El período de inscripción está fijo: dice "Sin definir" y siempre figura abierto.
- Ofrece también materias de 1.º año, y su alcance es 2.º y 3.º.
- Pasaron `enrollmentService` y `prerequisiteValidationService` a `src/services/estudiante/`. La regla acordada es que esa carpeta tiene solo el service del model Student: hay que decidir si se acepta o vuelven a la raíz de `services/`.

### Mesa Técnica

- Hay dos controllers de estudiantes: el del E1 (`StudentsController`, `/api/students`) y el del E2 (`StudentController`, `/api/estudiantes`). Hay que decidir cuál queda para cada cosa.
- Aprobar formalmente la tabla `SubjectSchedules` (horarios por materia y comisión), que entró con el E2.

## Números del proyecto (al 13/11/2026)

En `development` hay 123 commits sin contar los merges:

| Integrante | Equipo | Commits |
|---|---|---|
| Tomás González | E1 | 23 |
| Alan Ordóñez | E2 | 22 |
| Lorenzo Caro | E3 | 13 |
| Joaquín Olmedo | E1 | 12 |
| Agustín Coccia | E4 | 11 |
| Federico Petta | E3 | 10 |
| Damián Palacios | E1 | 7 |
| Carlos Luque Heredia | E1 | 7 |
| Facundo Pesa Giménez | E4 | 7 |
| Esteban Soria | E4 | 5 |
| Franco Petta | E1 | 4 |
| Jeremías Herrera | E4 | 2 |

En GitHub solo aparecen commits de 12 de los 19 integrantes. El trabajo de quienes no subieron nada desde su fork no se ve acá.
