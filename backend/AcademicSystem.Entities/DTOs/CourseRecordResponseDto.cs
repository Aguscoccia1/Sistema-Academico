namespace AcademicSystem.Entities.DTOs;

public class CourseRecordResponseDto
{
    private int id;
    private int studentSubjectId;
    private decimal? attendancePercentage;
    private decimal? numericGrade;
    private string? letterGrade;
    private string? finalStatus;
    private string? observations;
    private DateTime updatedAt;

    

    public int Id { get { return id; } set { id = value; } }
    public int StudentSubjectId { get { return studentSubjectId; } set { studentSubjectId = value; } }
    public decimal? AttendancePercentage { get { return attendancePercentage; } set { attendancePercentage = value; } }
    public decimal? NumericGrade { get { return numericGrade; } set { numericGrade = value; } }
    public string? LetterGrade { get { return letterGrade; } set { letterGrade = value; } }
    public string? FinalStatus { get { return finalStatus; } set { finalStatus = value; } }
    public string? Observations { get { return observations; } set { observations = value; } }
    public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }
}
