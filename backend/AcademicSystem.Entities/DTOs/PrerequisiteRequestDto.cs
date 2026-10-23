using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Entities.DTOs;

public class PrerequisiteRequestDto
{
    private int studyPlanId;
    private int subjectId;
    private int prerequisiteSubjectId;

    

    [Range(1, int.MaxValue)]
    public int StudyPlanId { get { return studyPlanId; } set { studyPlanId = value; } }

    [Range(1, int.MaxValue)]
    public int SubjectId { get { return subjectId; } set { subjectId = value; } }

    [Range(1, int.MaxValue)]
    public int PrerequisiteSubjectId { get { return prerequisiteSubjectId; } set { prerequisiteSubjectId = value; } }
}
