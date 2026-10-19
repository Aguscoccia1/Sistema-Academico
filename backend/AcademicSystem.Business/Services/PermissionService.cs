using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public class PermissionService : IPermissionService
{
    // Ninguna tabla de la base guarda permisos, así que la matriz vive acá, en un solo lugar.
    // Los módulos usan los mismos ids que las rutas del frontend.
    // IMPORTANTE: tiene que coincidir con la matriz que se cerró en S2-28.
    private static readonly Dictionary<string, Dictionary<string, string[]>> Matrix = new()
    {
        [RoleNames.AcademicSecretary] = new()
        {
            ["inicio"] = ["ver"],
            ["planes-de-estudio"] = ["ver", "crear", "editar", "baja", "restaurar"],
            ["docentes"] = ["ver", "crear", "editar"],
            ["inscripciones"] = ["ver"],
            ["usuarios"] = ["ver", "crear", "editar", "baja"]
        },
        [RoleNames.Student] = new()
        {
            ["inicio"] = ["ver"],
            ["inscripciones"] = ["ver", "crear"]
        }
    };

    public PermissionsResponseDto? GetByRole(string role)
    {
        if (!Matrix.TryGetValue(role, out var modules)) return null;

        return new PermissionsResponseDto
        {
            Role = role,
            Modules = modules
                .Select(m => new ModulePermissionDto { Module = m.Key, Actions = m.Value.ToList() })
                .ToList()
        };
    }

    public bool HasPermission(string role, string module, string action) =>
        Matrix.TryGetValue(role, out var modules)
        && modules.TryGetValue(module, out var actions)
        && actions.Contains(action);
}