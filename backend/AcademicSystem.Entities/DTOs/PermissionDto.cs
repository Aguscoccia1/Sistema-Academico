namespace AcademicSystem.Entities.DTOs;

public class ModulePermissionDto
{
    public string Module { get; init; } = string.Empty;
    public List<string> Actions { get; init; } = [];
}