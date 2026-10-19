using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IPermissionService
{
    PermissionsResponseDto? GetByRole(string role);
    bool HasPermission(string role, string module, string action);
}