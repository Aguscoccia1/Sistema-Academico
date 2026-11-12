using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public sealed class TeacherSubjectService(ITeacherSubjectRepository repository)
    : ITeacherSubjectService
{
    public Task<List<TeacherResponseDto>> GetAllTeachersAsync(
        CancellationToken cancellationToken = default) =>
        repository.GetAllTeachersAsync(cancellationToken);

    public Task<TeacherSubjectsResponseDto?> GetAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        repository.GetAssignmentsAsync(userId, cancellationToken);

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
        var existingSubjectIds = await repository.GetExistingSubjectIdsAsync(
            subjectIds, cancellationToken);
        if (existingSubjectIds.Count != subjectIds.Count)
        {
            return new UpdateTeacherSubjectsResultDto(
                true, false, "Una o más materias seleccionadas no existen.");
        }

        var teacherFound = await repository.ReplaceAssignmentsAsync(
            userId, assignments, cancellationToken);
        if (!teacherFound)
        {
            return new UpdateTeacherSubjectsResultDto(
                false, false, "No se encontró un usuario con rol Teacher.");
        }

        return new UpdateTeacherSubjectsResultDto(
            true, true, "Las materias del docente se guardaron correctamente.");
    }
}
