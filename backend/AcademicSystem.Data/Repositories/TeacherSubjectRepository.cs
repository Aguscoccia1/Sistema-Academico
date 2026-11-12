using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class TeacherSubjectRepository(AcademicSystemContext context)
    : ITeacherSubjectRepository
{
    private const string TeacherRoleName = "Teacher";

    public Task<List<TeacherResponseDto>> GetAllTeachersAsync(
        CancellationToken cancellationToken = default) =>
        context.Teachers
            .AsNoTracking()
            .OrderBy(teacher => teacher.User.LastName)
            .ThenBy(teacher => teacher.User.FirstName)
            .Select(teacher => new TeacherResponseDto(
                teacher.UserId,
                teacher.User.FirstName,
                teacher.User.LastName,
                teacher.User.Cuil,
                teacher.User.Email,
                teacher.User.IsActive))
            .ToListAsync(cancellationToken);

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

    public async Task<IReadOnlyList<int>> GetExistingSubjectIdsAsync(
        IReadOnlyList<int> subjectIds,
        CancellationToken cancellationToken = default) =>
        await context.Subjects
            .Where(subject => subjectIds.Contains(subject.Id))
            .Select(subject => subject.Id)
            .ToListAsync(cancellationToken);

    public async Task<bool> ReplaceAssignmentsAsync(
        int userId,
        IReadOnlyList<UpdateTeacherSubjectDto> assignments,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);

        var userExists = await context.Users
            .AnyAsync(
                item => item.Id == userId && item.Role.Name == TeacherRoleName,
                cancellationToken);
        if (!userExists)
            return false;

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

        return true;
    }
}
