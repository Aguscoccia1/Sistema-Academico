# Sistema Académico — Instituto Superior Cura Gabriel Brochero

Proyecto de Prácticas Profesionalizantes. Lo desarrollan 4 equipos sobre esta misma base.

El sistema tiene dos perfiles: **Secretario** y **Estudiante**.

> ¿Te trabaste con un error? Andá directo a [Si te aparece un error](#si-te-aparece-un-error).

---

## Las reglas que no se rompen

1. **Nunca** subas cambios a este repositorio directo. Tu trabajo va a **tu fork** y entra por Pull Request.
2. **Nunca** subas `node_modules/`, `dist/`, `bin/` ni `obj/`. Se generan solos.
3. **Nunca** cambies `backend/AcademicSystem.Api/appsettings.json` para poner tu servidor. Tu conexión se guarda solo en tu computadora: [cómo hacerlo](#si-tu-sql-server-tiene-otro-nombre).
4. **Nunca** uses `git push --force`. Si un error te lo sugiere, avisale al E1 antes.
5. Antes de abrir un Pull Request, `npm run build` tiene que pasar sin errores. Si tocaste el backend, también `dotnet build`.
6. Los archivos compartidos, como las rutas, el menú o `Program.cs`, se tocan **solo para agregar lo tuyo**. No les cambies el formato ni toques lo de otro equipo.
7. Los conflictos los resolvés **en tu computadora**, no con el botón de GitHub.

---

## Cómo funciona

Nadie sube directo a este repositorio. Cada integrante tiene **su propia copia en GitHub**, que se llama **fork**. Vos subís tu trabajo a tu fork y pedís que se integre con un **Pull Request**. El E1 lo revisa y lo mergea en `development`. Después, todos traen lo integrado desde ahí.

```
  tu computadora ── git push fork e2 ──> tu fork en GitHub
        ▲                                     │
        │                               Pull Request
        │                                     ▼
        └──── git merge origin/development ── development (este repositorio)
```

En tu computadora vas a tener dos conexiones a GitHub, que Git llama **remotos**:

| Remoto | Apunta a | Para qué lo usás |
|---|---|---|
| `origin` | `gonzaleztomi1978-design/Sistema-Academico`, este repositorio | Traer lo que ya está integrado |
| `fork` | `TU-USUARIO/Sistema-Academico`, tu copia | Subir tu trabajo |

Cada integrante tiene su propio fork, aunque sea del mismo equipo que otro. Tus compañeros reciben tus cambios cuando tu Pull Request entra a `development`.

---

## Primera vez: preparar todo

Se hace **una sola vez por computadora**. Los ejemplos usan la rama `e2`: cambiala por la de tu equipo, `e1`, `e2`, `e3` o `e4`. Los comandos funcionan igual en PowerShell y en Git Bash.

**1. Hacer tu fork.** En GitHub, con tu cuenta, entrá a https://github.com/gonzaleztomi1978-design/Sistema-Academico y tocá **Fork**. En el formulario, **destildá "Copy the production branch only"** y tocá **Create fork**.

**2. Elegir una carpeta fuera de OneDrive.** OneDrive bloquea archivos de Git mientras sincroniza y produce errores raros. Por ejemplo:

```bash
mkdir C:\Proyectos
cd C:\Proyectos
```

**3. Clonar este repositorio.** Se clona el del proyecto, no tu fork:

```bash
git clone https://github.com/gonzaleztomi1978-design/Sistema-Academico.git
cd Sistema-Academico
```

**4. Conectar tu fork.** Cambiá `TU-USUARIO` por tu usuario de GitHub. Si al crear el fork le cambiaste el nombre, copiá la dirección exacta del botón verde **Code** de tu fork.

```bash
git remote add fork https://github.com/TU-USUARIO/Sistema-Academico.git
git remote -v
```

Tienen que aparecer cuatro líneas: dos de `origin` con `gonzaleztomi1978-design` y dos de `fork` con tu usuario.

**5. Configurar tu nombre y el editor.** El editor es para que Git no te abra Vim, donde es fácil quedar trabado:

```bash
git config --global user.name "Nombre Apellido"
git config --global user.email "tumail@ejemplo.com"
git config --global core.editor "code --wait"
```

**6. Crear tu rama de equipo y subirla a tu fork:**

```bash
git fetch origin
git checkout -b e2 origin/e2
git push fork e2
```

Escribí `git checkout -b e2 origin/e2` completo. Un `git checkout e2` solo puede fallar.

**7. Levantar el frontend:**

```bash
npm install
npm run dev
```

Se abre en **http://localhost:5173**. Necesitás **Node.js 20 o superior**. Todavía no hay login: para cambiar entre Secretario y Estudiante usá el **simulador de roles** que aparece en pantalla.

### ¿Ya tenías el proyecto en tu computadora?

No hace falta clonarlo de nuevo. Corré `git remote -v` y fijate qué te aparece:

| Lo que ves | Qué hacer |
|---|---|
| Solo `origin`, con `gonzaleztomi1978-design` | `git remote add fork https://github.com/TU-USUARIO/Sistema-Academico.git` |
| `origin` con **tu** usuario, y no hay `fork` | `git remote add fork https://github.com/TU-USUARIO/Sistema-Academico.git` y después `git remote set-url origin https://github.com/gonzaleztomi1978-design/Sistema-Academico.git` |
| `origin` y `fork`, los dos con **tu** usuario | `git remote set-url origin https://github.com/gonzaleztomi1978-design/Sistema-Academico.git` |
| Solo `fork`, sin `origin` | `git remote add origin https://github.com/gonzaleztomi1978-design/Sistema-Academico.git` |

Después volvé a correr `git remote -v` para comprobarlo, y seguí desde el paso 6.

---

## Tu día a día

```bash
# 1. Pararte en tu rama
git checkout e2

# 2. Traer lo que ya integraron los demás. Hacelo SIEMPRE antes de empezar algo nuevo.
git fetch origin
git merge origin/development

# 3. Trabajar. Cuando terminás algo:
git status
git add .
git commit -m "E2: inscripción de estudiantes a primer año"
npm run build

# 4. Subir a tu fork
git push fork e2
```

- **Antes del `git add .`** mirá el `git status`. Si aparece algo que no tocaste, no lo agregues y preguntá.
- **El mensaje del commit** empieza con tu equipo y va en presente: `E2: agrega alta de docentes`.
- **Si el merge del paso 2 marca conflictos**, se resuelven en tu computadora: [cómo hacerlo](documentos/flujo-git.md#6-resolver-conflictos).

---

## Abrir el Pull Request

Cuando tu funcionalidad está terminada, compila y la probaste a mano:

1. Entrá a tu fork en GitHub. Tocá **Compare & pull request** o, si no aparece, **Pull requests > New pull request**.
2. Revisá el encabezado. Tiene que decir:
   - **base repository:** `gonzaleztomi1978-design/Sistema-Academico` y **base:** `development`
   - **head repository:** `TU-USUARIO/Sistema-Academico` y **compare:** `e2`
3. Poné un título como los commits, por ejemplo `E2: alta de docentes`. Completá la plantilla y tocá **Create pull request**.

También lo podés abrir directo con este link, cambiando `TU-USUARIO` y la rama:

```
https://github.com/gonzaleztomi1978-design/Sistema-Academico/compare/development...TU-USUARIO:e2?expand=1
```

**Después de abrirlo:**

- **Mientras esté abierto**, cada `git push fork e2` lo actualiza solo. No abras otro.
- **Si el E1 te pide cambios**, hacelos en tu rama, commiteá y volvé a hacer `git push fork e2`.
- **Cuando lo mergean**, seguí trabajando en la misma rama. El paso 2 del día a día te trae lo integrado.

---

## Base de datos y backend

### Qué necesitás instalar

- **SQL Server Express** y **SQL Server Management Studio**.
- **.NET 10 SDK**: https://dotnet.microsoft.com/download/dotnet/10.0

### 1. Crear la base en tu computadora

Cada uno tiene su propia copia de la base. Se crea con el script del repositorio:

1. Abrí Management Studio y conectate a tu servidor con **Windows Authentication**.
2. Abrí el archivo `database/AcademicSystem.sql` y apretá **F5**.

Se crea la base `AcademicSystem` con sus 22 tablas. Si el script cambia, lo volvés a correr: no borra nada ni duplica datos.

### 2. Levantar el backend

```bash
cd backend
dotnet run --project AcademicSystem.Api
```

Queda en **http://localhost:5000**. Para comprobar que se conectó a la base, abrí en el navegador:

**http://localhost:5000/api/health**

Tiene que mostrar `"status":"ok"` y `"roles":4`.

### Si tu SQL Server tiene otro nombre

El backend se conecta a `localhost\SQLEXPRESS`. Si tu servidor se llama distinto, **no cambies `appsettings.json`**: ese cambio les rompe la conexión a todos los demás. Guardá tu conexión solo en tu computadora con este comando, cambiando la parte de `Server`:

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:AcademicSystem" "Server=TU-SERVIDOR;Database=AcademicSystem;Trusted_Connection=True;TrustServerCertificate=True" --project AcademicSystem.Api
```

El nombre de tu servidor es el que aparece en **Server name** cuando te conectás desde Management Studio. Por ejemplo: `localhost`, `localhost\SQLEXPRESS` o el nombre de tu computadora.

### 3. Conectar el frontend con el backend

Creá un archivo `.env.local` en la raíz del proyecto, al lado de `package.json`, con esta línea:

```
VITE_API_BASE_URL=http://localhost:5000
```

Después reiniciá `npm run dev`. Ese archivo no se sube al repositorio.

Hoy las pantallas usan datos simulados. Cuando un equipo pase su pantalla a datos reales, llama a la API con `request('/api/...')` de `src/api/httpClient.js`.

### Cómo está armado el backend

```
backend/
├── AcademicSystem.Api/                 → endpoints (Controllers) y configuración
├── AcademicSystem.Business/Services/   → la lógica de cada funcionalidad
├── AcademicSystem.Data/Context/        → la conexión a la base (AcademicSystemContext)
└── AcademicSystem.Entities/
    ├── DTOs/                           → lo que la API recibe y devuelve
    └── Models/                         → una clase por cada tabla de la base
```

Cada funcionalidad recorre las cuatro capas en el mismo orden: el controller llama a un servicio, el servicio usa el contexto, y el contexto trabaja con los models. La API devuelve DTOs, nunca los models directamente. El ejemplo para copiar es el de `/api/health`: `HealthController`, `HealthService` y `HealthResponseDto`. Cada servicio nuevo se registra con una línea en `Program.cs`.

Las clases de `Entities/Models` y el contexto se generan automáticamente desde la base. **No los edites a mano.** Si la base cambia, se acuerda en la Mesa Técnica, se actualiza el script y el E1 los vuelve a generar con este comando:

```bash
cd backend
dotnet tool restore
dotnet ef dbcontext scaffold "Name=ConnectionStrings:AcademicSystem" Microsoft.EntityFrameworkCore.SqlServer --project AcademicSystem.Data --startup-project AcademicSystem.Api --context AcademicSystemContext --context-dir Context --context-namespace AcademicSystem.Data.Context --output-dir ../AcademicSystem.Entities/Models --namespace AcademicSystem.Entities.Models --no-onconfiguring --force
```

---

## Dónde va tu código

```
src/
├── core/        → layout, menú y ruteo. Lo mantiene el E1.
├── api/         → acceso a datos. Hoy son datos simulados. Lo mantiene el E1.
├── hooks/       → funciones reutilizables (useCrud). Lo mantiene el E1.
├── modules/
│   ├── secretario/pages/   → pantallas del Secretario (E2 y E3)
│   └── estudiante/pages/   → pantallas del Estudiante (E4)
└── styles/      → estilos compartidos
```

| Equipo | Qué hace | Su rama | Dónde trabaja |
|---|---|---|---|
| **E1** — Acceso e Integración | Login, usuarios, roles, permisos, integrar todo | `e1` | `src/core/`, `src/api/`, `src/hooks/` |
| **E2** — Secretaría | Inscripción a 1.º año, docentes, estudiantes por comisión | `e2` | `src/modules/secretario/pages/Docentes/` e `.../Inscripciones/` |
| **E3** — Gestión Académica | Planes de estudio, materias, correlatividades | `e3` | `src/modules/secretario/pages/PlanesEstudio/` |
| **E4** — Inscripciones | Consulta de materias e inscripción a 2.º y 3.º | `e4` | `src/modules/estudiante/pages/Inscripciones/` |

En el backend, cada equipo agrega sus endpoints en `backend/AcademicSystem.Api/Controllers/`, su lógica en `backend/AcademicSystem.Business/Services/` y sus DTOs en `backend/AcademicSystem.Entities/DTOs/`.

---

## Las ramas

```
    e1 ─┐
    e2 ─┤
    e3 ─┼─ PR ─> development ─ PR ─> testing ─ PR ─> production
    e4 ─┘
```

| Rama | Qué es | ¿Quién sube cambios? |
|---|---|---|
| `e1` `e2` `e3` `e4` | La rama de cada equipo. | Cada integrante, **a su fork**. |
| `development` | Donde se junta el trabajo de los 4 equipos. | Solo por Pull Request. Lo mergea el E1. |
| `testing` | Versión candidata: se prueba entera antes de darla por buena. | Solo por Pull Request desde `development`. |
| `production` | Versión estable. Es lo que se muestra y se entrega. | Solo por Pull Request desde `testing`. |

---

## Si te aparece un error

| El error dice | Qué pasó | Qué hacer |
|---|---|---|
| `Permission to gonzaleztomi1978-design/Sistema-Academico.git denied` o `error: 403` | Quisiste subir a este repositorio, y no se puede. | Subí a tu fork: `git push fork e2`. Si no tenés fork, hacé los pasos 1 y 4 de [Primera vez](#primera-vez-preparar-todo). |
| `'fork' does not appear to be a git repository` | Tu computadora no tiene tu fork conectado. | Hacé el paso 4 de [Primera vez](#primera-vez-preparar-todo). |
| `No such remote 'origin'` | Tu computadora no tiene este repositorio conectado. | `git remote add origin https://github.com/gonzaleztomi1978-design/Sistema-Academico.git` |
| `pathspec 'e2' did not match any file(s) known to git` | Tu computadora todavía no conoce la rama. | `git fetch origin` y `git checkout -b e2 origin/e2`. Si sigue fallando, revisá con `git remote -v` que `origin` diga `gonzaleztomi1978-design`. |
| `a branch named 'e2' already exists` | La rama ya la creaste antes. | `git checkout e2` |
| `! [rejected] e2 -> e2 (fetch first)` al hacer `git push fork e2` | Tu fork tiene commits que tu computadora no tiene, por ejemplo si subiste desde otra computadora. | `git pull fork e2` y después `git push fork e2`. Si aparecen conflictos o tenés dudas, avisale al E1 antes de seguir. |
| `Deletion of directory '.git/...' failed. Should I try again? (y/n)` | OneDrive tiene la carpeta bloqueada. | Respondé `n`. Pausá OneDrive y, cuando puedas, mové el proyecto fuera de OneDrive. |
| `Support for password authentication was removed` | GitHub ya no acepta tu contraseña. | Usá un token personal o el inicio de sesión por navegador. Ver [configuracion-github.md](documentos/configuracion-github.md#9-problemas-frecuentes). |
| `Your local changes to the following files would be overwritten` | Tenés cambios sin commitear que chocan. | Commitealos con `git add .` y `git commit`, o guardalos con `git stash`. Después volvé a intentar. |
| `CONFLICT (content): Merge conflict in ...` | Vos y otro equipo cambiaron las mismas líneas. | Resolvelo en tu computadora: [cómo hacerlo](documentos/flujo-git.md#6-resolver-conflictos). |
| `NETSDK1045` o `does not support targeting .NET 10.0` | Tenés una versión vieja de .NET. | Instalá el [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) y abrí una terminal nueva. |
| `/api/health` muestra `"unreachable"` | El backend no llega a tu base. | Revisá que la base exista. Si tu servidor no es `localhost\SQLEXPRESS`, [configurá tu conexión](#si-tu-sql-server-tiene-otro-nombre). |
| `git`, `npm` o `dotnet` "no se reconoce como nombre de un cmdlet" | Falta instalarlo, o la terminal se abrió antes de instalarlo. | Instalalo y abrí una terminal nueva. |

¿Te sigue fallando? Mandale al E1 una captura de la terminal con el comando y el error completos.

---

## Documentación

**De la cátedra**

- [Sistema Académico](documentos/Sistema%20Acad%C3%A9mico.md) — qué tiene que hacer el sistema.
- [Equipos Proyectos 2do](documentos/Equipos%20Proyectos%202do.md) — integrantes y responsabilidades de cada equipo.
- [continuacion.md](documentos/continuacion.md) — arquitectura del proyecto y convenciones de código.
- [diagrama UML](documentos/diagrama_uml_Sistema_Acad%C3%A9mico.md) — diagrama del sistema.

**De Git (las arma el E1)**

- [flujo-git.md](documentos/flujo-git.md) — guía completa del trabajo diario, con conflictos y casos raros.
- [integracion-y-releases.md](documentos/integracion-y-releases.md) — cómo integra y promueve el E1.
- [configuracion-github.md](documentos/configuracion-github.md) — configuración del repositorio en GitHub.
