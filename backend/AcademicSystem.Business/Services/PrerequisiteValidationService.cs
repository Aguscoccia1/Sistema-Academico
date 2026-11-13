using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public class PrerequisiteValidationService : IPrerequisiteValidationService
{
    private readonly AcademicSystemContext _context;

    public PrerequisiteValidationService(AcademicSystemContext context)
    {
        _context = context;
    }

    public async Task<PrerequisiteValidationResponseDto?> ValidateAsync(
        int userId,
        PrerequisiteValidationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        int? studentId = await _context.Students
            .AsNoTracking()
            .Where(student => student.UserId == userId)
            .Select(student => (int?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);
        bool studyPlanExists = await _context.StudyPlans
            .AsNoTracking()
            .AnyAsync(plan => plan.Id == request.StudyPlanId, cancellationToken);
        bool subjectExists = await _context.Subjects
            .AsNoTracking()
            .AnyAsync(subject => subject.Id == request.SubjectId, cancellationToken);

        if (studentId is null || !studyPlanExists || !subjectExists)
        {
            return null;
        }

        bool subjectBelongsToPlan = await _context.StudyPlanSubjects
            .AsNoTracking()
            .AnyAsync(item =>
                item.StudyPlanId == request.StudyPlanId && item.SubjectId == request.SubjectId,
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

        var prerequisiteList = await _context.Prerequisites
            .AsNoTracking()
            .Where(item => item.StudyPlanId == request.StudyPlanId && item.SubjectId == request.SubjectId)
            .Select(item => new
            {
                item.PrerequisiteSubjectId,
                SubjectName = item.PrerequisiteSubject.Name
            })
            .ToListAsync(cancellationToken);

        var prerequisiteSubjectIds = prerequisiteList
            .Select(item => item.PrerequisiteSubjectId)
            .ToList();

        var approvedPrerequisiteSubjectIds = await _context.StudentSubjects
            .AsNoTracking()
            .Where(item => item.StudentId == studentId.Value
                && prerequisiteSubjectIds.Contains(item.SubjectId)
                && item.CourseRecord != null
                && item.CourseRecord.FinalStatus == "Aprobado")
            .Select(item => item.SubjectId)
            .ToListAsync(cancellationToken);

        var fulfilledPrerequisites = prerequisiteList
            .Where(prerequisite => approvedPrerequisiteSubjectIds.Contains(prerequisite.PrerequisiteSubjectId))
            .Select(prerequisite => new PrerequisiteSubjectDto
            {
                SubjectId = prerequisite.PrerequisiteSubjectId,
                Name = prerequisite.SubjectName
            })
            .ToList();

        var missingPrerequisites = prerequisiteList
            .Where(prerequisite => !approvedPrerequisiteSubjectIds.Contains(prerequisite.PrerequisiteSubjectId))
            .Select(prerequisite => new PrerequisiteSubjectDto
            {
                SubjectId = prerequisite.PrerequisiteSubjectId,
                Name = prerequisite.SubjectName
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
