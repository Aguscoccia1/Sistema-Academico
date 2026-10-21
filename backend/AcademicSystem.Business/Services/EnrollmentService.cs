using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public sealed class EnrollmentService(AcademicSystemContext context) : IEnrollmentService
{
    public async Task<AvailableSubjectsResponseDto> GetAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await context.StudyPlanSubjects
            .AsNoTracking()
            .Where(planSubject =>
                planSubject.StudyPlan.IsActive &&
                planSubject.Subject.TeacherSubjects.Any())
            .Select(planSubject => new
            {
                planSubject.Subject.Id,
                planSubject.Subject.Name,
                planSubject.CourseYear
            })
            .Distinct()
            .OrderBy(subject => subject.CourseYear)
            .ThenBy(subject => subject.Name)
            .ToListAsync(cancellationToken);

        var subjects = rows
            .Select(row => new AvailableSubjectDto(row.Id, row.Id, row.Name, row.CourseYear))
            .ToList();

        return new AvailableSubjectsResponseDto(
            subjects.Count == 0
                ? "No hay materias disponibles para inscripción en el período vigente."
                : null,
            subjects);
    }
}
