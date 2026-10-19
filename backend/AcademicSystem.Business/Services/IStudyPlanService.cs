using AcademicSystem.Entities.Models;

namespace AcademicSystem.Business.Services;

public interface IStudyPlanService
{
    List<StudyPlan> GetAll();
    StudyPlan? GetById(int id);   
    bool ExistsByPlanCode(string planCode);
    bool ExistsByPlanCode(string planCode, int excludeId);
    StudyPlan Create(StudyPlan studyPlan);
    StudyPlan? Update(int id, StudyPlan changes);  
    bool ExistsById(int id);
    bool SubjectExists(int subjectId);
    bool ExistsSubjectInPlan(int studyPlanId, int subjectId);
    StudyPlanSubject AddSubject(StudyPlanSubject studyPlanSubject); 
}