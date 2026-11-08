using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/academic-secretaries")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class AcademicSecretariesController(IAcademicSecretaryService academicSecretaryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await academicSecretaryService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var secretary = await academicSecretaryService.GetByIdAsync(id, ct);
        return secretary is null ? NotFound() : Ok(secretary);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AcademicSecretaryRequestDto input, CancellationToken ct)
    {
        var result = await academicSecretaryService.CreateAsync(input, ct);
        return result.Status == ProfileResultStatus.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : ToError(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AcademicSecretaryRequestDto input, CancellationToken ct)
    {
        var result = await academicSecretaryService.UpdateAsync(id, input, ct);
        return result.Status == ProfileResultStatus.Success ? Ok(result.Data) : ToError(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await academicSecretaryService.DeleteAsync(id, ct);
        return result.Status == ProfileResultStatus.Success ? NoContent() : ToError(result);
    }

    private IActionResult ToError(ProfileResult<AcademicSecretaryResponseDto> result) => result.Status switch
    {
        ProfileResultStatus.NotFound => NotFound(),
        ProfileResultStatus.Conflict => Conflict(new { message = result.Message }),
        _ => BadRequest(new { message = result.Message })
    };
}
