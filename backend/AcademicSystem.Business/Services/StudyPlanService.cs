using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;
using AcademicSystem.Data.Context;

namespace AcademicSystem.Business.Services;

public class StudyPlanService : IStudyPlanService
{
    private readonly AcademicSystemContext _context;

    public StudyPlanService(AcademicSystemContext context)
    {
        _context = context;
    }

    public List<StudyPlan> GetAll()
    {
        return _context.StudyPlans.ToList();
    }

    public StudyPlan? GetById(int id)
    {
        return _context.StudyPlans
            .Include(sp => sp.StudyPlanSubjects)
                .ThenInclude(sps => sps.Subject)
            .FirstOrDefault(sp => sp.Id == id);
    }

    public bool ExistsByPlanCode(string planCode)
    {
        return _context.StudyPlans.Any(sp => sp.PlanCode == planCode);
    }

    public bool ExistsByPlanCode(string planCode, int excludeId)
    {
        return _context.StudyPlans.Any(sp => sp.PlanCode == planCode && sp.Id != excludeId);
    }

    public StudyPlan Create(StudyPlan studyPlan)
    {
        _context.StudyPlans.Add(studyPlan);
        _context.SaveChanges();
        return studyPlan;
    }

    public StudyPlan? Update(int id, StudyPlan changes)
    {
        StudyPlan? existing = _context.StudyPlans.FirstOrDefault(sp => sp.Id == id);

        if (existing == null)
        {
            return null;
        }

        existing.Name = changes.Name;
        existing.PlanCode = changes.PlanCode;
        existing.CareerDurationYears = changes.CareerDurationYears;
        existing.AttendanceMode = changes.AttendanceMode;
        existing.TotalWorkloadHours = changes.TotalWorkloadHours;
        existing.StartYear = changes.StartYear;
        existing.EndYear = changes.EndYear;
        existing.IsActive = changes.IsActive;

        _context.SaveChanges();
        return existing;
    }

    public bool ExistsById(int id)
    {
        return _context.StudyPlans.Any(sp => sp.Id == id);
    }

    public bool SubjectExists(int subjectId)
    {
        return _context.Subjects.Any(s => s.Id == subjectId);
    }

    public bool ExistsSubjectInPlan(int studyPlanId, int subjectId)
    {
        return _context.StudyPlanSubjects.Any(sps => sps.StudyPlanId == studyPlanId && sps.SubjectId == subjectId);
    }

    public StudyPlanSubject AddSubject(StudyPlanSubject studyPlanSubject)
    {
        _context.StudyPlanSubjects.Add(studyPlanSubject);
        _context.SaveChanges();

        return _context.StudyPlanSubjects
            .Include(sps => sps.Subject)
            .First(sps => sps.Id == studyPlanSubject.Id);
    }
}