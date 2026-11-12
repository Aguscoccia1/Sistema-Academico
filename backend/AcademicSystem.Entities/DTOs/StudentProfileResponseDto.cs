namespace AcademicSystem.Entities.DTOs;

public record StudentProfileResponseDto(int Id, int UserId, string FirstName, string LastName, string Dni, string RecordNumber);
