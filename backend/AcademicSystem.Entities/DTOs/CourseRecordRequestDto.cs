using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Entities.DTOs;

public class CourseRecordRequestDto
{
    private int studentSubjectId;
    private decimal? attendancePercentage;
    private decimal? numericGrade;
    private string? letterGrade;
    private string? finalStatus;
    private string? observations;

    

    [Range(1, int.MaxValue)]
    public int StudentSubjectId { get { return studentSubjectId; } set { studentSubjectId = value; } }

    [Range(0, 100)]
    public decimal? AttendancePercentage { get { return attendancePercentage; } set { attendancePercentage = value; } }

    [Range(0, 10)]
    public decimal? NumericGrade { get { return numericGrade; } set { numericGrade = value; } }

    [StringLength(20)]
    public string? LetterGrade { get { return letterGrade; } set { letterGrade = value; } }

    [StringLength(30)]
    public string? FinalStatus { get { return finalStatus; } set { finalStatus = value; } }

    [StringLength(1000)]
    public string? Observations { get { return observations; } set { observations = value; } }
}
