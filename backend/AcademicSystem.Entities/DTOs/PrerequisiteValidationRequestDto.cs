using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Entities.DTOs;

public class PrerequisiteValidationRequestDto
{
    private int studentId;
    private int studyPlanId;
    private int subjectId;

    [Range(1, int.MaxValue)]
    public int StudentId { get { return studentId; } set { studentId = value; } }

    [Range(1, int.MaxValue)]
    public int StudyPlanId { get { return studyPlanId; } set { studyPlanId = value; } }

    [Range(1, int.MaxValue)]
    public int SubjectId { get { return subjectId; } set { subjectId = value; } }
}
