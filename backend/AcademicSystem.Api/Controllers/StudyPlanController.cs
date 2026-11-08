using AcademicSystem.Entities.DTOs;
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
    private readonly IPrerequisiteService _prerequisiteService;
    private readonly ILogger<StudyPlanController> _logger;

    public StudyPlanController(IStudyPlanService studyPlanService, IPrerequisiteService prerequisiteService, ILogger<StudyPlanController> logger)
    {
        _studyPlanService = studyPlanService;
        _prerequisiteService = prerequisiteService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        try
        {
            return Ok(await _studyPlanService.GetAllAsync(ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar los planes de estudio");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            StudyPlanDetailResponseDto? studyPlan = await _studyPlanService.GetByIdAsync(id, ct);
 
            if (studyPlan == null)
            {
                return NotFound("No se encontró el plan de estudio.");
            }
 
            return Ok(studyPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar el plan de estudio {Id}", id);
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StudyPlanRequestDto newStudyPlan, CancellationToken ct)
    {
        try
        {
            string? error = ValidatePlan(newStudyPlan);
 
            if (error != null)
            {
                return BadRequest(error);
            }
 
            if (await _studyPlanService.ExistsByPlanCodeAsync(newStudyPlan.PlanCode, null, ct))
            {
                return Conflict("Ya existe un plan de estudio con ese código.");
            }
 
            StudyPlanResponseDto savedStudyPlan = await _studyPlanService.CreateAsync(newStudyPlan, ct);
 
            return CreatedAtAction(nameof(GetById), new { id = savedStudyPlan.Id }, savedStudyPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el plan de estudio");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudyPlanRequestDto updatedStudyPlan, CancellationToken ct)
    {
        try
        {
            string? error = ValidatePlan(updatedStudyPlan);
 
            if (error != null)
            {
                return BadRequest(error);
            }
 
            if (await _studyPlanService.ExistsByPlanCodeAsync(updatedStudyPlan.PlanCode, id, ct))
            {
                return Conflict("Ya existe otro plan de estudio con ese código.");
            }
 
            StudyPlanResponseDto? savedStudyPlan = await _studyPlanService.UpdateAsync(id, updatedStudyPlan, ct);
 
            if (savedStudyPlan == null)
            {
                return NotFound("No se encontró el plan de estudio.");
            }
 
            return Ok(savedStudyPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el plan de estudio {Id}", id);
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPost("{id}/subjects")]
    public async Task<IActionResult> AddSubject(int id, [FromBody] StudyPlanSubjectRequestDto newPlanSubject, CancellationToken ct)
    {
        try
        {
            string? error = ValidatePlanSubject(newPlanSubject);
 
            if (error != null)
            {
                return BadRequest(error);
            }
 
            if (!await _studyPlanService.PlanExistsAsync(id, ct))
            {
                return NotFound("No se encontró el plan de estudio.");
            }
 
            if (!await _studyPlanService.SubjectExistsAsync(newPlanSubject.SubjectId, ct))
            {
                return NotFound("No se encontró la materia.");
            }
 
            if (await _studyPlanService.SubjectInPlanAsync(id, newPlanSubject.SubjectId, ct))
            {
                return Conflict("La materia ya forma parte de este plan de estudio.");
            }
 
            StudyPlanSubjectResponseDto savedPlanSubject = await _studyPlanService.AddSubjectAsync(id, newPlanSubject, ct);
 
            return CreatedAtAction(nameof(GetById), new { id }, savedPlanSubject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar la materia al plan de estudio {Id}", id);
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpGet("{id}/prerequisites")]
    public async Task<IActionResult> GetPrerequisites(int id, CancellationToken ct)
    {
        try
        {
            List<PrerequisiteItemDto>? prerequisites = await _prerequisiteService.GetByPlanAsync(id, ct);
 
            if (prerequisites == null)
            {
                return NotFound("No se encontró el plan de estudio.");
            }
 
            return Ok(prerequisites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar las correlatividades del plan de estudio {Id}", id);
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPut("{id}/subjects/{subjectId}/prerequisites")]
    public async Task<IActionResult> ReplacePrerequisites(int id, int subjectId, [FromBody] ReplacePrerequisitesRequestDto request, CancellationToken ct)
    {
        try
        {
            List<PrerequisiteItemDto> prerequisites = await _prerequisiteService.ReplaceAsync(id, subjectId, request.PrerequisiteSubjectIds, ct);
 
            return Ok(prerequisites);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar las correlatividades de la materia {SubjectId} del plan de estudio {Id}", subjectId, id);
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    private static string? ValidatePlan(StudyPlanRequestDto request)
    {
        if (request == null
            || string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.PlanCode))
        {
            return "Debe ingresar el nombre y el código del plan.";
        }
 
        if (request.StartYear <= 0)
        {
            return "Debe ingresar un año de inicio válido.";
        }
 
        if (request.EndYear != null && request.EndYear < request.StartYear)
        {
            return "El año de fin no puede ser anterior al año de inicio.";
        }
 
        return null;
    }
 
    private static string? ValidatePlanSubject(StudyPlanSubjectRequestDto request)
    {
        if (request == null || request.SubjectId <= 0)
        {
            return "Debe indicar la materia.";
        }
 
        if (request.CourseYear < 1 || request.CourseYear > 10)
        {
            return "El año de cursada tiene que estar entre 1 y 10.";
        }
 
        if (request.Term is not null && request.Term != 1 && request.Term != 2)
        {
            return "El cuatrimestre tiene que ser 1, 2 o vacío si la materia es anual.";
        }
 
        return null;
    }
    
}


