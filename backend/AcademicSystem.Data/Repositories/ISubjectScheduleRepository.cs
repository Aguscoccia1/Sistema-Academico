using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Data.Repositories;

public interface ISubjectScheduleRepository
{
    Task<bool> SubjectExistsAsync(
        int subjectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HorarioMateriaResponseDto>> GetAsync(
        int subjectId,
        string section,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteSuperpuestoAsync(
        int subjectId,
        string section,
        string day,
        TimeOnly start,
        TimeOnly end,
        CancellationToken cancellationToken = default);

    void Add(SubjectSchedule schedule);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
