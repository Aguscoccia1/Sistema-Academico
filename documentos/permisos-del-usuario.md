# Endpoint de permisos del usuario

Documenta `GET /api/auth/permissions`, que devuelve los módulos y acciones permitidos para el rol del usuario autenticado. El frontend lo usa para armar el menú. Pertenece al Equipo 1 (Acceso e Integración), historia de control de acceso por rol.

## Qué hace

Lee el rol del token JWT y devuelve qué módulos puede ver ese rol y qué acciones puede hacer en cada uno. El rol nunca se recibe por parámetro: sale siempre del token.

## Contrato

| | |
|---|---|
| Método y ruta | `GET /api/auth/permissions` |
| Autenticación | Header `Authorization: Bearer <token>` |
| Parámetros | Ninguno |

### Respuestas

| Código | Cuándo |
|---|---|
| 200 | Token válido y rol presente en la matriz. |
| 401 | Sin token, o con token vencido. Lo resuelve el `FallbackPolicy` de `Program.cs`. |
| 403 | Token válido, pero el rol no figura en la matriz. |

### Ejemplo de respuesta 200 (Secretario)

```json
{
  "role": "AcademicSecretary",
  "modules": [
    { "module": "inicio", "actions": ["ver"] },
    { "module": "planes-de-estudio", "actions": ["ver", "crear", "editar", "baja", "restaurar"] },
    { "module": "docentes", "actions": ["ver", "crear", "editar"] },
    { "module": "inscripciones", "actions": ["ver"] },
    { "module": "usuarios", "actions": ["ver", "crear", "editar", "baja"] }
  ]
}
```

Los ids de módulo coinciden con las rutas del frontend. Los módulos y acciones de este ejemplo deben mantenerse iguales a la matriz de permisos acordada por el equipo.

## Dónde está el código

| Capa | Archivo |
|---|---|
| Entities | `AcademicSystem.Entities/DTOs/ModulePermissionDto.cs` |
| Entities | `AcademicSystem.Entities/DTOs/PermissionsResponseDto.cs` |
| Business | `AcademicSystem.Business/Services/IPermissionService.cs` |
| Business | `AcademicSystem.Business/Services/PermissionService.cs` |
| Api | `AcademicSystem.Api/Controllers/PermissionsController.cs` |
| Api | `AcademicSystem.Api/Program.cs` (una línea de registro del servicio) |

## Cómo funciona

```
GET /api/auth/permissions
  → FallbackPolicy                (sin token válido: 401)
  → PermissionsController         (lee el claim "role" del token)
  → PermissionService.GetByRole   (busca el rol en la matriz)
  ← PermissionsResponseDto        (rol + módulos + acciones, o 403 si el rol no está)
```

- La matriz de permisos vive como `private static readonly` dentro de `PermissionService`, porque ninguna de las tablas de la base guarda permisos.
- El rol se lee con `User.FindFirst("role")`, no con `ClaimTypes.Role`, porque `Program.cs` configura `MapInboundClaims = false` y `RoleClaimType = "role"`.
- `PermissionService` también expone `HasPermission(role, module, action)`, pensado para validar acciones puntuales desde otros servicios.

## Cómo probarlo

Con la API corriendo (`dotnet run --project AcademicSystem.Api` desde `backend/`), en PowerShell:

```powershell
$login = Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/auth/login -ContentType "application/json" -Body '{"email":"maria@uni.edu","password":"admin"}'
Invoke-RestMethod -Uri http://localhost:5000/api/auth/permissions -Headers @{ Authorization = "Bearer $($login.token)" }
```

Para el estudiante, reemplazá el body del login por `{"email":"loko@uni.edu","password":"1234"}`.

## Resultados de la prueba

Completar con lo observado al ejecutar cada caso.

| Caso | Resultado esperado | Resultado obtenido |
|---|---|---|
| Login como Secretario y consulta con su token | 200 con los módulos de Secretario | _completar_ |
| Login como Estudiante y consulta con su token | 200 solo con "inicio" e "inscripciones" | _completar_ |
| Consulta sin header `Authorization` | 401 | _completar_ |
| Consulta con token vencido o inválido | 401 | _completar_ |

## Notas para quien lo mantenga

- Si se agrega un módulo nuevo, hay que registrarlo en `PermissionService` para los roles que lo usan. Ver la guía `proteger-un-endpoint.md`.
- Si cambia el nombre del claim del rol en `JwtTokenService`, hay que actualizar la lectura en `PermissionsController`.
- La respuesta se serializa en camelCase (`role`, `modules`, `module`, `actions`), que es lo que debe esperar el frontend.