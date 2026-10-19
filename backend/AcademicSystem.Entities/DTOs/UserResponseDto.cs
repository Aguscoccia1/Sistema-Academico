namespace AcademicSystem.Entities.DTOs;

// Lo que la API devuelve de un usuario. Nunca incluye el hash de la contraseña.
public record UserResponseDto(int Id, string FirstName, string LastName, string Cuil, DateOnly? BirthDate,
    string? Address, int RoleId, string RoleName, string Email, string? Phone,
    string? EmergencyContactPhone, bool IsActive);
