namespace AcademicSystem.Entities.DTOs;

public class StudentProfileRequestDto
{
    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Dni { get; set; }

    public string? RecordNumber { get; set; }
}
