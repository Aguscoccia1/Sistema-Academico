namespace AcademicSystem.Entities.DTOs;

public class PrerequisiteValidationResponseDto
{
    private int studentId;
    private int studyPlanId;
    private int subjectId;
    private bool canEnroll;
    private List<PrerequisiteSubjectDto> fulfilledPrerequisites = new List<PrerequisiteSubjectDto>();
    private List<PrerequisiteSubjectDto> missingPrerequisites = new List<PrerequisiteSubjectDto>();
    private string message = "";

    public int StudentId { get { return studentId; } set { studentId = value; } }
    public int StudyPlanId { get { return studyPlanId; } set { studyPlanId = value; } }
    public int SubjectId { get { return subjectId; } set { subjectId = value; } }
    public bool CanEnroll { get { return canEnroll; } set { canEnroll = value; } }
    public List<PrerequisiteSubjectDto> FulfilledPrerequisites { get { return fulfilledPrerequisites; } set { fulfilledPrerequisites = value; } }
    public List<PrerequisiteSubjectDto> MissingPrerequisites { get { return missingPrerequisites; } set { missingPrerequisites = value; } }
    public string Message { get { return message; } set { message = value; } }
}
