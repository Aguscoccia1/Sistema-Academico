using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AcademicSystem.Business.Services;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class SubjectController : ControllerBase
{
    private readonly ISubjectService _subjectService;
    private readonly ILogger<SubjectController> _logger;

    public SubjectController(ISubjectService subjectService, ILogger<SubjectController> logger)
    {
        _subjectService = subjectService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        try
        {
            return Ok(await _subjectService.GetAllAsync(ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar las materias");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            SubjectResponseDto? subject = await _subjectService.GetByIdAsync(id, ct);
 
            if (subject == null)
            {
                return NotFound("No se encontró la materia.");
            }
 
            return Ok(subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar la materia {Id}", id);
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SubjectRequestDto newSubject, CancellationToken ct)
    {
        try
        {
            string? error = ValidateSubject(newSubject);
 
            if (error != null)
            {
                return BadRequest(error);
            }
 
            SubjectResponseDto savedSubject = await _subjectService.CreateAsync(newSubject, ct);
 
            return CreatedAtAction(nameof(GetById), new { id = savedSubject.Id }, savedSubject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la materia");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SubjectRequestDto updatedSubject, CancellationToken ct)
    {
        try
        {
            string? error = ValidateSubject(updatedSubject);
 
            if (error != null)
            {
                return BadRequest(error);
            }
 
            SubjectResponseDto? savedSubject = await _subjectService.UpdateAsync(id, updatedSubject, ct);
 
            if (savedSubject == null)
            {
                return NotFound("No se encontró la materia.");
            }
 
            return Ok(savedSubject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la materia {Id}", id);
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }

    private static string? ValidateSubject(SubjectRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return "Debe ingresar el nombre de la materia.";
        }
 
        if (request.LectureHours < 0 || request.TotalHours < 0)
        {
            return "Las horas no pueden ser negativas.";
        }
 
        return null;
    }
    
}