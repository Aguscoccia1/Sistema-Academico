using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
 
namespace AcademicSystem.Business.Services;
 
public class StudyPlanService : IStudyPlanService
{
    private readonly IStudyPlanRepository _studyPlanRepository;
    private readonly ISubjectRepository _subjectRepository;
 
    public StudyPlanService(IStudyPlanRepository studyPlanRepository, ISubjectRepository subjectRepository)
    {
        _studyPlanRepository = studyPlanRepository;
        _subjectRepository = subjectRepository;
    }
 
    public async Task<List<StudyPlanResponseDto>> GetAllAsync(CancellationToken ct)
    {
        List<StudyPlan> studyPlans = await _studyPlanRepository.GetAllAsync(ct);
 
        return studyPlans
            .Select(ToResponse)
            .ToList();
    }
 
    public async Task<StudyPlanDetailResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        StudyPlan? studyPlan = await _studyPlanRepository.GetByIdWithSubjectsAsync(id, ct);
 
        return studyPlan == null ? null : ToDetailResponse(studyPlan);
    }
 
    public async Task<StudyPlanResponseDto> CreateAsync(StudyPlanRequestDto request, CancellationToken ct)
    {
        StudyPlan studyPlan = new StudyPlan();
        CopyFields(request, studyPlan);
 
        _studyPlanRepository.Add(studyPlan);
        await _studyPlanRepository.SaveChangesAsync(ct);
 
        return ToResponse(studyPlan);
    }
 
    public async Task<StudyPlanResponseDto?> UpdateAsync(int id, StudyPlanRequestDto request, CancellationToken ct)
    {
        StudyPlan? studyPlan = await _studyPlanRepository.GetByIdAsync(id, ct);
 
        if (studyPlan == null)
        {
            return null;
        }
 
        CopyFields(request, studyPlan);
        await _studyPlanRepository.SaveChangesAsync(ct);
 
        return ToResponse(studyPlan);
    }
 
    public async Task<StudyPlanSubjectResponseDto> AddSubjectAsync(int studyPlanId, StudyPlanSubjectRequestDto request, CancellationToken ct)
    {
        StudyPlanSubject planSubject = new StudyPlanSubject
        {
            StudyPlanId = studyPlanId,
            SubjectId = request.SubjectId,
            CourseYear = request.CourseYear,
            Term = request.Term
        };
 
        _studyPlanRepository.AddSubject(planSubject);
        await _studyPlanRepository.SaveChangesAsync(ct);
 
        // se vuelve a consultar para traer los datos de la materia en la respuesta
        StudyPlanSubject savedPlanSubject = (await _studyPlanRepository.GetPlanSubjectAsync(planSubject.Id, ct))!;
 
        return ToPlanSubjectResponse(savedPlanSubject);
    }
 
    public async Task<bool> PlanExistsAsync(int id, CancellationToken ct)
    {
        return await _studyPlanRepository.ExistsAsync(id, ct);
    }
 
    // excludeId permite ignorar el propio plan cuando se está editando (null = no excluir ninguno)
    public async Task<bool> ExistsByPlanCodeAsync(string planCode, int? excludeId, CancellationToken ct)
    {
        return await _studyPlanRepository.ExistsByPlanCodeAsync(planCode, excludeId, ct);
    }
 
    public async Task<bool> SubjectExistsAsync(int subjectId, CancellationToken ct)
    {
        return await _subjectRepository.ExistsAsync(subjectId, ct);
    }
 
    public async Task<bool> SubjectInPlanAsync(int studyPlanId, int subjectId, CancellationToken ct)
    {
        return await _studyPlanRepository.ExistsSubjectInPlanAsync(studyPlanId, subjectId, ct);
    }
 
    private static void CopyFields(StudyPlanRequestDto source, StudyPlan target)
    {
        target.Name = source.Name;
        target.PlanCode = source.PlanCode;
        target.CareerDurationYears = source.CareerDurationYears;
        target.AttendanceMode = source.AttendanceMode;
        target.TotalWorkloadHours = source.TotalWorkloadHours;
        target.StartYear = source.StartYear;
        target.EndYear = source.EndYear;
        target.IsActive = source.IsActive;
    }
 
    private static StudyPlanResponseDto ToResponse(StudyPlan studyPlan)
    {
        return new StudyPlanResponseDto
        {
            Id = studyPlan.Id,
            Name = studyPlan.Name,
            PlanCode = studyPlan.PlanCode,
            CareerDurationYears = studyPlan.CareerDurationYears,
            AttendanceMode = studyPlan.AttendanceMode,
            TotalWorkloadHours = studyPlan.TotalWorkloadHours,
            StartYear = studyPlan.StartYear,
            EndYear = studyPlan.EndYear,
            IsActive = studyPlan.IsActive
        };
    }
 
    private static StudyPlanDetailResponseDto ToDetailResponse(StudyPlan studyPlan)
    {
        return new StudyPlanDetailResponseDto
        {
            Id = studyPlan.Id,
            Name = studyPlan.Name,
            PlanCode = studyPlan.PlanCode,
            CareerDurationYears = studyPlan.CareerDurationYears,
            AttendanceMode = studyPlan.AttendanceMode,
            TotalWorkloadHours = studyPlan.TotalWorkloadHours,
            StartYear = studyPlan.StartYear,
            EndYear = studyPlan.EndYear,
            IsActive = studyPlan.IsActive,
            Subjects = studyPlan.StudyPlanSubjects
                .OrderBy(sps => sps.CourseYear)
                .ThenBy(sps => sps.Term)
                .Select(ToPlanSubjectResponse)
                .ToList()
        };
    }
 
    private static StudyPlanSubjectResponseDto ToPlanSubjectResponse(StudyPlanSubject planSubject)
    {
        return new StudyPlanSubjectResponseDto
        {
            Id = planSubject.Id,
            StudyPlanId = planSubject.StudyPlanId,
            SubjectId = planSubject.SubjectId,
            CourseYear = planSubject.CourseYear,
            Term = planSubject.Term,
            Subject = SubjectService.ToResponse(planSubject.Subject)
        };
    }
}