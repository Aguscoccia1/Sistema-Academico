using AcademicSystem.Entities.DTOs;
 
namespace AcademicSystem.Business.Services;
 
public interface ISubjectService
{
    Task<List<SubjectResponseDto>> GetAllAsync(CancellationToken ct);
    Task<SubjectResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<SubjectResponseDto> CreateAsync(SubjectRequestDto request, CancellationToken ct);
    Task<SubjectResponseDto?> UpdateAsync(int id, SubjectRequestDto request, CancellationToken ct);   // null si la materia no existe
}