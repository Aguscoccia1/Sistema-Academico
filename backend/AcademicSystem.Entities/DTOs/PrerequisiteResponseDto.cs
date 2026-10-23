namespace AcademicSystem.Entities.DTOs;

public class PrerequisiteResponseDto
{
    private int id;
    private int studyPlanId;
    private int subjectId;
    private int prerequisiteSubjectId;



    public int Id { get { return id; } set { id = value; } }
    public int StudyPlanId { get { return studyPlanId; } set { studyPlanId = value; } }
    public int SubjectId { get { return subjectId; } set { subjectId = value; } }
    public int PrerequisiteSubjectId { get { return prerequisiteSubjectId; } set { prerequisiteSubjectId = value; } }
}
