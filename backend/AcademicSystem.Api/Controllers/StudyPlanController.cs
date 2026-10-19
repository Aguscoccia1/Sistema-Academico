using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AcademicSystem.Business.Services;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class StudyPlanController : ControllerBase
{
    private readonly IStudyPlanService _studyPlanService;
    private readonly ILogger<StudyPlanController> _logger;

    public StudyPlanController(IStudyPlanService studyPlanService, ILogger<StudyPlanController> logger)
    {
        _studyPlanService = studyPlanService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            List<StudyPlan> studyPlans = _studyPlanService.GetAll();

            return Ok(studyPlans.Select(sp => new StudyPlanResponseDto
            {
                Id = sp.Id,
                Name = sp.Name,
                PlanCode = sp.PlanCode,
                CareerDurationYears = sp.CareerDurationYears,
                AttendanceMode = sp.AttendanceMode,
                TotalWorkloadHours = sp.TotalWorkloadHours,
                StartYear = sp.StartYear,
                EndYear = sp.EndYear,
                IsActive = sp.IsActive
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        try
        {
            StudyPlan? sp = _studyPlanService.GetById(id);

            if (sp == null)
            {
                return NotFound("No se encontró el plan de estudio.");
            }

            StudyPlanDetailResponseDto response = new StudyPlanDetailResponseDto
            {
                Id = sp.Id,
                Name = sp.Name,
                PlanCode = sp.PlanCode,
                CareerDurationYears = sp.CareerDurationYears,
                AttendanceMode = sp.AttendanceMode,
                TotalWorkloadHours = sp.TotalWorkloadHours,
                StartYear = sp.StartYear,
                EndYear = sp.EndYear,
                IsActive = sp.IsActive,
                Subjects = sp.StudyPlanSubjects
                    .OrderBy(x => x.CourseYear)
                    .ThenBy(x => x.Term)
                    .Select(x => new StudyPlanSubjectResponseDto
                    {
                        Id = x.Id,
                        StudyPlanId = x.StudyPlanId,
                        SubjectId = x.SubjectId,
                        CourseYear = x.CourseYear,
                        Term = x.Term,
                        Subject = new SubjectResponseDto
                        {
                            Id = x.Subject.Id,
                            Name = x.Subject.Name,
                            OrderNumber = x.Subject.OrderNumber,
                            Format = x.Subject.Format,
                            LectureHours = x.Subject.LectureHours,
                            TotalHours = x.Subject.TotalHours,
                            Course = x.Subject.Course
                        }
                    })
                    .ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] StudyPlanRequestDto newStudyPlan)
    {
        try
        {
            if (newStudyPlan == null
                || string.IsNullOrWhiteSpace(newStudyPlan.Name)
                || string.IsNullOrWhiteSpace(newStudyPlan.PlanCode))
            {
                return BadRequest("Debe ingresar el nombre y el código del plan.");
            }

            if (newStudyPlan.StartYear <= 0)
            {
                return BadRequest("Debe ingresar un año de inicio válido.");
            }

            if (newStudyPlan.EndYear != null && newStudyPlan.EndYear < newStudyPlan.StartYear)
            {
                return BadRequest("El año de fin no puede ser anterior al año de inicio.");
            }

            if (_studyPlanService.ExistsByPlanCode(newStudyPlan.PlanCode))
            {
                return Conflict("Ya existe un plan de estudio con ese código.");
            }

            StudyPlan studyPlan = new StudyPlan
            {
                Name = newStudyPlan.Name,
                PlanCode = newStudyPlan.PlanCode,
                CareerDurationYears = newStudyPlan.CareerDurationYears,
                AttendanceMode = newStudyPlan.AttendanceMode,
                TotalWorkloadHours = newStudyPlan.TotalWorkloadHours,
                StartYear = newStudyPlan.StartYear,
                EndYear = newStudyPlan.EndYear,
                IsActive = newStudyPlan.IsActive
            };

            StudyPlan created = _studyPlanService.Create(studyPlan);

            StudyPlanResponseDto response = new StudyPlanResponseDto
            {
                Id = created.Id,
                Name = created.Name,
                PlanCode = created.PlanCode,
                CareerDurationYears = created.CareerDurationYears,
                AttendanceMode = created.AttendanceMode,
                TotalWorkloadHours = created.TotalWorkloadHours,
                StartYear = created.StartYear,
                EndYear = created.EndYear,
                IsActive = created.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] StudyPlanRequestDto updatedStudyPlan)
    {
        try
        {
            if (updatedStudyPlan == null
                || string.IsNullOrWhiteSpace(updatedStudyPlan.Name)
                || string.IsNullOrWhiteSpace(updatedStudyPlan.PlanCode))
            {
                return BadRequest("Debe ingresar el nombre y el código del plan.");
            }

            if (updatedStudyPlan.StartYear <= 0)
            {
                return BadRequest("Debe ingresar un año de inicio válido.");
            }

            if (updatedStudyPlan.EndYear != null && updatedStudyPlan.EndYear < updatedStudyPlan.StartYear)
            {
                return BadRequest("El año de fin no puede ser anterior al año de inicio.");
            }

            // se excluye el propio plan para que no choque con su mismo código
            if (_studyPlanService.ExistsByPlanCode(updatedStudyPlan.PlanCode, id))
            {
                return Conflict("Ya existe otro plan de estudio con ese código.");
            }

            StudyPlan changes = new StudyPlan
            {
                Name = updatedStudyPlan.Name,
                PlanCode = updatedStudyPlan.PlanCode,
                CareerDurationYears = updatedStudyPlan.CareerDurationYears,
                AttendanceMode = updatedStudyPlan.AttendanceMode,
                TotalWorkloadHours = updatedStudyPlan.TotalWorkloadHours,
                StartYear = updatedStudyPlan.StartYear,
                EndYear = updatedStudyPlan.EndYear,
                IsActive = updatedStudyPlan.IsActive
            };

            StudyPlan? updated = _studyPlanService.Update(id, changes);

            if (updated == null)
            {
                return NotFound("No se encontró el plan de estudio.");
            }

            StudyPlanResponseDto response = new StudyPlanResponseDto
            {
                Id = updated.Id,
                Name = updated.Name,
                PlanCode = updated.PlanCode,
                CareerDurationYears = updated.CareerDurationYears,
                AttendanceMode = updated.AttendanceMode,
                TotalWorkloadHours = updated.TotalWorkloadHours,
                StartYear = updated.StartYear,
                EndYear = updated.EndYear,
                IsActive = updated.IsActive
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPost("{id}/subjects")]
    public IActionResult AddSubject(int id, [FromBody] StudyPlanSubjectRequestDto newPlanSubject)
    {
        try
        {
            if (newPlanSubject == null || newPlanSubject.SubjectId <= 0)
            {
                return BadRequest("Debe indicar la materia.");
            }

            if (newPlanSubject.CourseYear < 1 || newPlanSubject.CourseYear > 10)
            {
                return BadRequest("El año de cursada tiene que estar entre 1 y 10.");
            }

            if (newPlanSubject.Term is not null && newPlanSubject.Term != 1 && newPlanSubject.Term != 2)
            {
                return BadRequest("El cuatrimestre tiene que ser 1, 2 o vacío si la materia es anual.");
            }

            if (!_studyPlanService.ExistsById(id))
            {
                return NotFound("No se encontró el plan de estudio.");
            }

            if (!_studyPlanService.SubjectExists(newPlanSubject.SubjectId))
            {
                return NotFound("No se encontró la materia.");
            }

            if (_studyPlanService.ExistsSubjectInPlan(id, newPlanSubject.SubjectId))
            {
                return Conflict("La materia ya forma parte de este plan de estudio.");
            }

            StudyPlanSubject planSubject = new StudyPlanSubject
            {
                StudyPlanId = id,
                SubjectId = newPlanSubject.SubjectId,
                CourseYear = newPlanSubject.CourseYear,
                Term = newPlanSubject.Term
            };

            StudyPlanSubject savedPlanSubject = _studyPlanService.AddSubject(planSubject);

            StudyPlanSubjectResponseDto response = new StudyPlanSubjectResponseDto
            {
                Id = savedPlanSubject.Id,
                StudyPlanId = savedPlanSubject.StudyPlanId,
                SubjectId = savedPlanSubject.SubjectId,
                CourseYear = savedPlanSubject.CourseYear,
                Term = savedPlanSubject.Term,
                Subject = new SubjectResponseDto
                {
                    Id = savedPlanSubject.Subject.Id,
                    Name = savedPlanSubject.Subject.Name,
                    OrderNumber = savedPlanSubject.Subject.OrderNumber,
                    Format = savedPlanSubject.Subject.Format,
                    LectureHours = savedPlanSubject.Subject.LectureHours,
                    TotalHours = savedPlanSubject.Subject.TotalHours,
                    Course = savedPlanSubject.Subject.Course
                }
            };

            return CreatedAtAction(nameof(GetById), new { id = id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }
}
