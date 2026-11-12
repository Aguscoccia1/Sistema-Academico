using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public sealed class PrerequisiteValidationService(
    IPrerequisiteValidationRepository repository) : IPrerequisiteValidationService
{
    public async Task<PrerequisiteValidationResponseDto?> ValidateAsync(
        int userId,
        PrerequisiteValidationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        int? studentId = await repository.GetStudentIdByUserIdAsync(userId, cancellationToken);
        bool studyPlanExists = await repository.StudyPlanExistsAsync(
            request.StudyPlanId, cancellationToken);
        bool subjectExists = await repository.SubjectExistsAsync(
            request.SubjectId, cancellationToken);

        if (studentId is null || !studyPlanExists || !subjectExists)
        {
            return null;
        }

        bool subjectBelongsToPlan = await repository.SubjectBelongsToPlanAsync(
            request.StudyPlanId, request.SubjectId, cancellationToken);

        if (!subjectBelongsToPlan)
        {
            return new PrerequisiteValidationResponseDto
            {
                StudentId = studentId.Value,
                StudyPlanId = request.StudyPlanId,
                SubjectId = request.SubjectId,
                CanEnroll = false,
                Message = "La materia seleccionada no pertenece al plan de estudios indicado."
            };
        }

        var prerequisiteList = await repository.GetPrerequisitesAsync(
            request.StudyPlanId, request.SubjectId, cancellationToken);

        var prerequisiteSubjectIds = prerequisiteList
            .Select(item => item.SubjectId)
            .ToList();

        var approvedPrerequisiteSubjectIds =
            await repository.GetApprovedPrerequisiteSubjectIdsAsync(
                studentId.Value, prerequisiteSubjectIds, cancellationToken);

        var fulfilledPrerequisites = prerequisiteList
            .Where(prerequisite => approvedPrerequisiteSubjectIds.Contains(prerequisite.SubjectId))
            .Select(prerequisite => new PrerequisiteSubjectDto
            {
                SubjectId = prerequisite.SubjectId,
                Name = prerequisite.Name
            })
            .ToList();

        var missingPrerequisites = prerequisiteList
            .Where(prerequisite => !approvedPrerequisiteSubjectIds.Contains(prerequisite.SubjectId))
            .Select(prerequisite => new PrerequisiteSubjectDto
            {
                SubjectId = prerequisite.SubjectId,
                Name = prerequisite.Name
            })
            .ToList();

        var canEnroll = missingPrerequisites.Count == 0;

        return new PrerequisiteValidationResponseDto
        {
            StudentId = studentId.Value,
            StudyPlanId = request.StudyPlanId,
            SubjectId = request.SubjectId,
            CanEnroll = canEnroll,
            FulfilledPrerequisites = fulfilledPrerequisites,
            MissingPrerequisites = missingPrerequisites,
            Message = canEnroll
                ? "El estudiante cumple todas las correlativas de la materia."
                : "El estudiante no cumple todas las correlativas requeridas."
        };
    }
}
