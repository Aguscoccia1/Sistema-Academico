namespace AcademicSystem.Entities.DTOs;

public class PermissionsResponseDto
{
    public string Role { get; init; } = string.Empty;
    public List<ModulePermissionDto> Modules { get; init; } = [];
}