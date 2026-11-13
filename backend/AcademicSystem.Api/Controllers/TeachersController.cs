using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/teachers")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public sealed class TeachersController(ITeacherSubjectService teacherSubjectService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await teacherSubjectService.GetAllTeachersAsync(cancellationToken));

    [HttpGet("{userId:int}/subjects")]
    public async Task<ActionResult<TeacherSubjectsResponseDto>> GetAssignments(
        int userId,
        CancellationToken cancellationToken)
    {
        if (userId <= 0)
            return BadRequest("El ID del usuario docente debe ser válido.");

        var result = await teacherSubjectService.GetAssignmentsAsync(
            userId, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{userId:int}/subjects")]
    public async Task<ActionResult<UpdateTeacherSubjectsResultDto>> ReplaceAssignments(
        int userId,
        [FromBody] UpdateTeacherSubjectsRequestDto? request,
        CancellationToken cancellationToken)
    {
        if (userId <= 0 || request?.Assignments is null)
            return BadRequest("El docente y la lista de materias son obligatorios.");

        var result = await teacherSubjectService.ReplaceAssignmentsAsync(
            userId, request.Assignments, cancellationToken);

        if (!result.TeacherFound)
            return NotFound(result.Message);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }
}
