namespace AcademicSystem.Entities.DTOs;

public class StudentSubjectResponseDto
{
    private int id;
    private int studentId;
    private int subjectId;
    private string section = "";


    

    public int Id { get { return id; } set { id = value; } }
    public int StudentId { get { return studentId; } set { studentId = value; } }
    public int SubjectId { get { return subjectId; } set { subjectId = value; } }
    public string Section { get { return section; } set { section = value; } }
}
