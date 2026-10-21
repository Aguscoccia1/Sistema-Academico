namespace AcademicSystem.Entities.DTOs;

public class PrerequisiteSubjectDto
{
    private int subjectId;
    private string name = "";

    public int SubjectId { get { return subjectId; } set { subjectId = value; } }
    public string Name { get { return name; } set { name = value; } }
}
