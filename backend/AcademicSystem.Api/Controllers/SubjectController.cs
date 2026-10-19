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
    public IActionResult GetAll()
    {
        try
        {
            List<SubjectResponseDto> subjects = _subjectService.GetAll();

            return Ok(subjects);
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
            Subject? subject = _subjectService.GetById(id);

            if (subject == null)
            {
                return NotFound("No se encontró la materia.");
            }

            SubjectResponseDto response = new SubjectResponseDto
            {
                Id = subject.Id,
                Name = subject.Name,
                OrderNumber = subject.OrderNumber,
                Format = subject.Format,
                LectureHours = subject.LectureHours,
                TotalHours = subject.TotalHours,
                Course = subject.Course
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
    public IActionResult Create([FromBody] SubjectRequestDto newSubject)
    {
        try
        {
            if (newSubject == null || string.IsNullOrWhiteSpace(newSubject.Name))
            {
                return BadRequest("Debe ingresar el nombre de la materia.");
            }

            if (newSubject.LectureHours < 0 || newSubject.TotalHours < 0)
            {
                return BadRequest("Las horas no pueden ser negativas.");
            }

            Subject subject = new Subject
            {
                Name = newSubject.Name,
                OrderNumber = newSubject.OrderNumber,
                Format = newSubject.Format,
                LectureHours = newSubject.LectureHours,
                TotalHours = newSubject.TotalHours,
                Course = newSubject.Course
            };

            Subject created = _subjectService.Create(subject);

            SubjectResponseDto response = new SubjectResponseDto
            {
                Id = created.Id,
                Name = created.Name,
                OrderNumber = created.OrderNumber,
                Format = created.Format,
                LectureHours = created.LectureHours,
                TotalHours = created.TotalHours,
                Course = created.Course
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
    public IActionResult Update(int id, [FromBody] SubjectRequestDto updatedSubject)
    {
        try
        {
            if (updatedSubject == null || string.IsNullOrWhiteSpace(updatedSubject.Name))
            {
                return BadRequest("Debe ingresar el nombre de la materia.");
            }

            if (updatedSubject.LectureHours < 0 || updatedSubject.TotalHours < 0)
            {
                return BadRequest("Las horas no pueden ser negativas.");
            }

            Subject changes = new Subject
            {
                Name = updatedSubject.Name,
                OrderNumber = updatedSubject.OrderNumber,
                Format = updatedSubject.Format,
                LectureHours = updatedSubject.LectureHours,
                TotalHours = updatedSubject.TotalHours,
                Course = updatedSubject.Course
            };

            Subject? updated = _subjectService.Update(id, changes);

            if (updated == null)
            {
                return NotFound("No se encontró la materia.");
            }

            SubjectResponseDto response = new SubjectResponseDto
            {
                Id = updated.Id,
                Name = updated.Name,
                OrderNumber = updated.OrderNumber,
                Format = updated.Format,
                LectureHours = updated.LectureHours,
                TotalHours = updated.TotalHours,
                Course = updated.Course
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }
}