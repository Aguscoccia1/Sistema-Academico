using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public class PrerequisiteValidationService(
    IPrerequisiteValidationRepository prerequisiteValidationRepository,
    IEnrollmentRepository enrollmentRepository) : IPrerequisiteValidationService
{
    private const string ApprovedStatus = "Aprobado";

    public async Task<PrerequisiteValidationResponseDto?> ValidateAsync(
        int userId,
        PrerequisiteValidationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        int? studentId = await enrollmentRepository.GetStudentIdByUserIdAsync(userId, cancellationToken);
        bool studyPlanExists = await prerequisiteValidationRepository.StudyPlanExistsAsync(request.StudyPlanId, cancellationToken);
        bool subjectExists = await prerequisiteValidationRepository.SubjectExistsAsync(request.SubjectId, cancellationToken);

        if (studentId is null || !studyPlanExists || !subjectExists)
        {
            return null;
        }

        bool subjectBelongsToPlan = await prerequisiteValidationRepository.SubjectBelongsToPlanAsync(
            request.StudyPlanId,
            request.SubjectId,
            cancellationToken);

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

        var prerequisiteList = await prerequisiteValidationRepository.GetPrerequisitesAsync(
            request.StudyPlanId,
            request.SubjectId,
            cancellationToken);

        var approvedPrerequisiteSubjectIds = await prerequisiteValidationRepository.GetApprovedSubjectIdsAsync(
            studentId.Value,
            prerequisiteList.Select(prerequisite => prerequisite.SubjectId).ToList(),
            ApprovedStatus,
            cancellationToken);

        var fulfilledPrerequisites = prerequisiteList
            .Where(prerequisite => approvedPrerequisiteSubjectIds.Contains(prerequisite.SubjectId))
            .ToList();

        var missingPrerequisites = prerequisiteList
            .Where(prerequisite => !approvedPrerequisiteSubjectIds.Contains(prerequisite.SubjectId))
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
