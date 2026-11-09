using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class StudentsController(IStudentProfileService studentProfileService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await studentProfileService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var student = await studentProfileService.GetByIdAsync(id, ct);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    public async Task<IActionResult> Create(StudentProfileRequestDto input, CancellationToken ct)
    {
        var result = await studentProfileService.CreateAsync(input, ct);
        return result.Status == ProfileResultStatus.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : ToError(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, StudentProfileRequestDto input, CancellationToken ct)
    {
        var result = await studentProfileService.UpdateAsync(id, input, ct);
        return result.Status == ProfileResultStatus.Success ? Ok(result.Data) : ToError(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await studentProfileService.DeleteAsync(id, ct);
        return result.Status == ProfileResultStatus.Success ? NoContent() : ToError(result);
    }

    private IActionResult ToError(ProfileResult<StudentProfileResponseDto> result) => result.Status switch
    {
        ProfileResultStatus.NotFound => NotFound(),
        ProfileResultStatus.Conflict => Conflict(new { message = result.Message }),
        _ => BadRequest(new { message = result.Message })
    };
}
