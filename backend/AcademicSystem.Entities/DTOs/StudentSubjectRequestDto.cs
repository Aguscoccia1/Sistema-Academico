using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Entities.DTOs;

public class StudentSubjectRequestDto
{
    private int studyPlanId;
    private int subjectId;
    private string section = "";


    

    [Range(1, int.MaxValue)]
    public int StudyPlanId { get { return studyPlanId; } set { studyPlanId = value; } }

    [Range(1, int.MaxValue)]
    public int SubjectId { get { return subjectId; } set { subjectId = value; } }

    [Required, StringLength(20)]
    public string Section { get { return section; } set { section = value; } }
}
