using System;
using System.Collections.Generic;

namespace AcademicSystem.Entities.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? OrderNumber { get; set; }

    public string? Format { get; set; }

    public int? LectureHours { get; set; }

    public int? TotalHours { get; set; }

    public string? Course { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<Prerequisite> PrerequisitePrerequisiteSubjects { get; set; } = new List<Prerequisite>();

    public virtual ICollection<Prerequisite> PrerequisiteSubjects { get; set; } = new List<Prerequisite>();

    public virtual ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();

    public virtual ICollection<SubjectSchedule> SubjectSchedules { get; set; } = new List<SubjectSchedule>();

    public virtual ICollection<StudyPlanSubject> StudyPlanSubjects { get; set; } = new List<StudyPlanSubject>();

    public virtual ICollection<SubjectProgram> SubjectPrograms { get; set; } = new List<SubjectProgram>();

    public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
}
