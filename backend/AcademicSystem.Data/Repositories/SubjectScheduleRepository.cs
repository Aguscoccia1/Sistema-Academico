using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class SubjectScheduleRepository(AcademicSystemContext context)
    : ISubjectScheduleRepository
{
    public Task<bool> SubjectExistsAsync(
        int subjectId,
        CancellationToken cancellationToken = default) =>
        context.Subjects.AnyAsync(subject => subject.Id == subjectId, cancellationToken);

    public async Task<IReadOnlyList<HorarioMateriaResponseDto>> GetAsync(
        int subjectId,
        string section,
        CancellationToken cancellationToken = default) =>
        await context.SubjectSchedules
            .AsNoTracking()
            .Where(schedule =>
                schedule.SubjectId == subjectId &&
                schedule.Section == section)
            .Select(schedule => new HorarioMateriaResponseDto(
                schedule.Id,
                schedule.SubjectId,
                schedule.Section,
                schedule.Day,
                schedule.StartTime,
                schedule.EndTime))
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteSuperpuestoAsync(
        int subjectId,
        string section,
        string day,
        TimeOnly start,
        TimeOnly end,
        CancellationToken cancellationToken = default) =>
        context.SubjectSchedules.AnyAsync(
            schedule =>
                schedule.SubjectId == subjectId &&
                schedule.Section == section &&
                schedule.Day == day &&
                schedule.StartTime < end &&
                schedule.EndTime > start,
            cancellationToken);

    public void Add(SubjectSchedule schedule) =>
        context.SubjectSchedules.Add(schedule);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
