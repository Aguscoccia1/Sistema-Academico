namespace AcademicSystem.Entities.Models;

public partial class SubjectSchedule
{
    public int Id { get; set; }

    public int SubjectId { get; set; }

    public string Section { get; set; } = null!;

    public string Day { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual Subject Subject { get; set; } = null!;
}
