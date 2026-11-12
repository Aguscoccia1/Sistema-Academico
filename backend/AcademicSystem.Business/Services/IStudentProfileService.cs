using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IStudentProfileService
{
    Task<List<StudentProfileResponseDto>> GetAllAsync(CancellationToken ct);

    Task<StudentProfileResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<ProfileResult<StudentProfileResponseDto>> CreateAsync(StudentProfileRequestDto input, CancellationToken ct);

    Task<ProfileResult<StudentProfileResponseDto>> UpdateAsync(int id, StudentProfileRequestDto input, CancellationToken ct);

    Task<ProfileResult<StudentProfileResponseDto>> DeleteAsync(int id, CancellationToken ct);
}
