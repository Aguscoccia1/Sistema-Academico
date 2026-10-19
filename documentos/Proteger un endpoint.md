# Cómo proteger un endpoint nuevo — guía para E2, E3 y E4

Esta guía explica qué dejó armado el E1 (Acceso e Integración) para el control de acceso, y qué tiene que hacer cada equipo cuando agrega un endpoint nuevo al backend. Si vas a crear un controller, leé las secciones 3 y 7 antes de abrir el Pull Request.

> **La regla de fondo:** ocultar un botón o una ruta en el frontend mejora la navegación, pero **no protege nada**. Quien protege es el backend, en cada endpoint.

---

## 1. Qué construyó el E1

| Pieza | Qué hace | Dónde está | Estado |
|---|---|---|---|
| Login con JWT | `POST /api/auth/login` valida las credenciales y devuelve el token. | `AuthController`, `AuthService`, `JwtTokenService` | Disponible |
| Autorización por defecto | Todos los endpoints exigen usuario autenticado. Sin token válido, responde **401**. | `Program.cs` (`FallbackPolicy`) | Disponible |
| Roles | Las constantes `RoleNames.AcademicSecretary` y `RoleNames.Student`. | `AcademicSystem.Business/Services` | Disponible |
| Gestión de usuarios | CRUD de usuarios, solo para el secretario. | `UsersController` | Disponible |
| Permisos del usuario | `GET /api/auth/permissions` devuelve los módulos y acciones del rol logueado, para armar el menú. | `PermissionsController`, `PermissionService` | Llega con el PR de S2-31 |

> **Nota para el E1:** cuando se mergee el PR de S2-31, cambiar la última fila a "Disponible". El detalle del endpoint está en [permisos-del-usuario.md](./permisos-del-usuario.md).

---

## 2. Cómo viaja una petición

```
Petición con token (header Authorization: Bearer <token>)
  → Program.cs valida el JWT (firma y vencimiento) ......... si falla: 401
  → FallbackPolicy: ¿hay un usuario autenticado? ........... si no: 401
  → [Authorize(Roles = ...)]: ¿su rol está permitido? ...... si no: 403
  → Tu controller → tu servicio → la base de datos
```

| Código | Qué significa |
|---|---|
| **401** | No hay sesión válida: falta el token, o el token venció. El token vencido da 401 en el momento, sin margen de gracia. |
| **403** | Hay sesión válida, pero ese rol no puede hacer eso. |

El token lleva el id del usuario en el claim `"sub"` y el rol en el claim `"role"`.

---

## 3. Paso a paso para proteger tu endpoint

### 3.1. Exigir login: no hay nada que hacer

Todo endpoint nuevo ya exige login por defecto. **No agregues `[AllowAnonymous]`.** Hoy solo lo usan el login (`AuthController`) y `HealthController`.

### 3.2. Restringir por rol

Se declara en la clase del controller, igual que `StudentsController` y `UsersController`:

```csharp
using AcademicSystem.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/subjects")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class SubjectsController(ISubjectService subjectService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await subjectService.GetAllAsync(ct));
}
```

Ese ejemplo es ilustrativo: `ISubjectService` es un nombre inventado. Usá tu propio servicio.

Si el usuario está logueado pero su rol no corresponde, la API responde **403** sin devolver datos. Si un endpoint lo pueden usar los dos roles, no pongas `Roles`: alcanza con el login por defecto.

### 3.3. Negar una acción a todos los roles

Si hay una ruta que ningún rol debe poder usar, devolvé `Forbid()`. Es lo que hace `RolesController` con sus rutas bloqueadas:

```csharp
[HttpDelete("{id:int}")]
public IActionResult Delete(int id) => Forbid();
```

### 3.4. Endpoints del estudiante: verificá que el dato sea suyo

Un estudiante no puede ver datos de otro cambiando el id en la URL. El rol solo no alcanza: hay que comparar el dueño del recurso con el usuario del token.

```csharp
[HttpGet("mine")]
[Authorize(Roles = RoleNames.Student)]
public async Task<IActionResult> GetMine(CancellationToken ct)
{
    var userId = int.Parse(User.FindFirst("sub")!.Value);
    return Ok(await enrollmentService.GetByUserAsync(userId, ct));
}
```

Cuando el endpoint recibe un id, buscá el recurso, y si no es del usuario del token devolvé `Forbid()`. Nunca confíes en un id que venga por la URL o por el body para saber quién es el usuario.

### 3.5. Devolvé DTOs, no models

La API devuelve y recibe DTOs de `AcademicSystem.Entities/DTOs`, nunca los models generados de la base. Los models no se editan a mano. Si tu endpoint recibe un model directamente como entrada, el cliente puede mandar campos que no debería controlar.

### 3.6. Registrar tu servicio

Una línea en `Program.cs`, **al final del bloque de Business**, para reducir conflictos con los otros equipos:

```csharp
builder.Services.AddScoped<IMiServicio, MiServicio>();
```

No cambies nada más de `Program.cs`: es un archivo compartido y se toca solo para agregar lo tuyo.

### 3.7. Registrar tu módulo en la matriz de permisos

La matriz define qué módulos y acciones tiene cada rol, y es lo que usa `GET /api/auth/permissions` para armar el menú. Vive en `AcademicSystem.Business/Services/PermissionService.cs`. Si tu módulo no está ahí, no aparece en el menú de nadie.

Agregá una línea por módulo, solo en los roles que lo usan. El id del módulo tiene que ser el mismo que usa la ruta del frontend:

```csharp
[RoleNames.AcademicSecretary] = new()
{
    // ... módulos existentes ...
    ["mi-modulo"] = ["ver", "crear", "editar"]
}
```

La fuente de verdad es el código de `PermissionService.cs`, no esta guía. Si dudás de qué acciones corresponden, consultalo en la Mesa Técnica antes de agregarlas.

---

## 4. Cómo probar que quedó bien protegido

Con la API corriendo (`dotnet run --project AcademicSystem.Api` desde `backend/`), en PowerShell:

```powershell
# Login como secretario
$login = Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/auth/login -ContentType "application/json" -Body '{"email":"maria@uni.edu","password":"admin"}'
$headers = @{ Authorization = "Bearer $($login.token)" }
Invoke-RestMethod -Uri http://localhost:5000/api/tu-ruta -Headers $headers
```

Probá siempre estos tres casos:

| Caso | Cómo | Resultado esperado |
|---|---|---|
| Rol correcto | Token de `maria@uni.edu` (secretario) | 200 con los datos |
| Rol equivocado | Login con `loko@uni.edu` / `1234` (estudiante) y llamar a un endpoint de secretario | 403 |
| Sin sesión | Llamar sin el header `Authorization` | 401 |

PowerShell trata el 401 y el 403 como error y muestra el código en el mensaje.

---

## 5. Si te pasa esto, hacé esto

| Qué ves | Qué pasó | Qué hacer |
|---|---|---|
| Da **403** aunque tenés el rol correcto | Estás leyendo el rol con `ClaimTypes.Role`. Este proyecto desactiva el mapeo de claims (`MapInboundClaims = false`), así que el rol viaja como `"role"`. | Usá `[Authorize(Roles = ...)]`, o leé el rol con `User.FindFirst("role")?.Value`. |
| Da **401** con un token que parecía válido | El token venció, o el header está mal armado. | Volvé a loguearte. El header tiene que ser `Authorization: Bearer <token>`, con la palabra `Bearer` y un espacio. |
| Da **500** con un mensaje de que no puede resolver un servicio | Te olvidaste de registrarlo. | Agregá tu línea `AddScoped` en `Program.cs` (3.6). |
| Tu endpoint responde sin iniciar sesión | Quedó un `[AllowAnonymous]` en el controller o en la acción. | Sacalo. Solo el login y el health son públicos. |
| Tu módulo no aparece en el menú | No está en la matriz de permisos. | Agregalo en `PermissionService.cs` (3.7). |
| `Jwt:Key` no encontrada al levantar la API | Falta la clave local. | Seguí la sección "Clave del login" del README. |

---

## 6. Reglas de oro

- **El frontend no protege.** Todo control de acceso se valida en el backend.
- **No agregues `[AllowAnonymous]`** a un endpoint nuevo.
- **No leas el rol con `ClaimTypes.Role`.** En este proyecto el claim se llama `"role"`.
- **No recibas el id del usuario por parámetro.** Salvo que el endpoint sea de gestión para el secretario, el usuario sale siempre del token.
- **No devuelvas ni recibas models.** Usá DTOs.
- **`Program.cs` se toca solo para agregar tu línea.** Sin cambiar formato ni líneas de otros equipos.

---

## 7. Checklist antes del Pull Request

- [ ] El controller o la acción tiene `[Authorize(Roles = ...)]` si es de un solo rol.
- [ ] No hay ningún `[AllowAnonymous]` que no corresponda.
- [ ] Si el dato es de un estudiante, se verifica contra el `"sub"` del token.
- [ ] El módulo está registrado en `PermissionService.cs`.
- [ ] El servicio está registrado en `Program.cs`.
- [ ] Probé los tres casos de la sección 4: 200, 403 y 401.
- [ ] Devuelvo DTOs, no models.
- [ ] `dotnet build backend/AcademicSystem.sln` pasa sin errores.
- [ ] El mensaje del commit empieza con tu equipo, por ejemplo `E3: agrega CRUD de materias`.

El resto del flujo (traer `development`, subir a tu fork y abrir el PR) está en [flujo-git.md](./flujo-git.md). El E1 revisa el PR, así que si algo de seguridad no cierra, te lo va a devolver con comentarios.

**Dudas:** consultá al E1 (Acceso e Integración) o llevalas a la Mesa Técnica.