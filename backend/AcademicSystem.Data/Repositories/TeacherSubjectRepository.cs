using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class TeacherSubjectRepository(AcademicSystemContext context)
    : ITeacherSubjectRepository
{
    private const string TeacherRoleName = "Teacher";

    public async Task<TeacherSubjectsResponseDto?> GetAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(item => item.Id == userId && item.Role.Name == TeacherRoleName)
            .Select(item => new
            {
                item.Id,
                item.FirstName,
                item.LastName,
                item.Email
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            return null;

        var teacherId = await context.Teachers
            .Where(item => item.UserId == userId)
            .Select(item => (int?)item.Id)
            .SingleOrDefaultAsync(cancellationToken);

        List<TeacherSubjectAssignmentDto> assignments = [];
        if (teacherId is not null)
        {
            assignments = await context.TeacherSubjects
                .AsNoTracking()
                .Where(item => item.TeacherId == teacherId.Value)
                .OrderBy(item => item.Subject.Name)
                .ThenBy(item => item.Section)
                .Select(item => new TeacherSubjectAssignmentDto(
                    item.SubjectId,
                    item.Subject.Name,
                    item.Section))
                .ToListAsync(cancellationToken);
        }

        return new TeacherSubjectsResponseDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            assignments);
    }

    public async Task<UpdateTeacherSubjectsResultDto> ReplaceAssignmentsAsync(
        int userId,
        IReadOnlyList<UpdateTeacherSubjectDto> assignments,
        CancellationToken cancellationToken = default)
    {
        if (assignments.Any(item =>
                item.SubjectId <= 0 ||
                item.Section is not ("A" or "B")))
        {
            return new UpdateTeacherSubjectsResultDto(
                true, false, "Cada materia debe tener un ID válido y comisión A o B.");
        }

        var duplicates = assignments
            .GroupBy(item => (item.SubjectId, item.Section))
            .Any(group => group.Count() > 1);
        if (duplicates)
        {
            return new UpdateTeacherSubjectsResultDto(
                true, false, "No se puede repetir la misma materia y comisión.");
        }

        var subjectIds = assignments
            .Select(item => item.SubjectId)
            .Distinct()
            .ToList();
        var existingSubjectIds = await context.Subjects
            .Where(subject => subjectIds.Contains(subject.Id))
            .Select(subject => subject.Id)
            .ToListAsync(cancellationToken);

        if (existingSubjectIds.Count != subjectIds.Count)
        {
            return new UpdateTeacherSubjectsResultDto(
                true, false, "Una o más materias seleccionadas no existen.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);

        var userExists = await context.Users
            .AnyAsync(
                item => item.Id == userId && item.Role.Name == TeacherRoleName,
                cancellationToken);
        if (!userExists)
            return new UpdateTeacherSubjectsResultDto(
                false, false, "No se encontró un usuario con rol Teacher.");

        var teacher = await context.Teachers
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (teacher is null)
        {
            teacher = new Teacher { UserId = userId };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync(cancellationToken);
        }

        var currentAssignments = await context.TeacherSubjects
            .Where(item => item.TeacherId == teacher.Id)
            .ToListAsync(cancellationToken);
        context.TeacherSubjects.RemoveRange(currentAssignments);
        context.TeacherSubjects.AddRange(assignments.Select(item => new TeacherSubject
        {
            TeacherId = teacher.Id,
            SubjectId = item.SubjectId,
            Section = item.Section
        }));

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new UpdateTeacherSubjectsResultDto(
            true, true, "Las materias del docente se guardaron correctamente.");
    }
}
