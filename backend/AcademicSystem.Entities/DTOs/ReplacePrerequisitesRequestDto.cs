namespace AcademicSystem.Entities.DTOs;
 
public class ReplacePrerequisitesRequestDto
{
    public List<int> PrerequisiteSubjectIds { get; set; } = new List<int>();
}