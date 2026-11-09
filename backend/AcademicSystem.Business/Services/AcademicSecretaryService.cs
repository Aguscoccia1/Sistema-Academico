using System.Text.Json;
using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Business.Services;

public class AcademicSecretaryService(IAcademicSecretaryRepository secretaryRepository, IUserRepository userRepository)
    : IAcademicSecretaryService
{
    public Task<List<AcademicSecretaryResponseDto>> GetAllAsync(CancellationToken ct) => secretaryRepository.GetAllAsync(ct);

    public Task<AcademicSecretaryResponseDto?> GetByIdAsync(int id, CancellationToken ct) => secretaryRepository.GetByIdAsync(id, ct);

    public async Task<ProfileResult<AcademicSecretaryResponseDto>> CreateAsync(AcademicSecretaryRequestDto input, CancellationToken ct)
    {
        var error = ValidateTitles(input.TitlesJson);
        if (error is not null) return Failure(ProfileResultStatus.InvalidRequest, error);

        var userRole = await userRepository.GetRoleNameOfUserAsync(input.UserId, ct);
        if (userRole is null) return Failure(ProfileResultStatus.InvalidRequest, "El usuario indicado no existe.");
        if (userRole != RoleNames.AcademicSecretary)
            return Failure(ProfileResultStatus.InvalidRequest, "El usuario debe tener el rol AcademicSecretary para crear un perfil de secretario académico.");

        if (await secretaryRepository.ExistsForUserAsync(input.UserId, ct))
            return Failure(ProfileResultStatus.Conflict, "Ese usuario ya tiene perfil de secretario académico.");

        var secretary = new AcademicSecretary { UserId = input.UserId, TitlesJson = Clean(input.TitlesJson) };

        secretaryRepository.Add(secretary);
        await secretaryRepository.SaveChangesAsync(ct);

        return Success(secretary);
    }

    public async Task<ProfileResult<AcademicSecretaryResponseDto>> UpdateAsync(int id, AcademicSecretaryRequestDto input, CancellationToken ct)
    {
        var error = ValidateTitles(input.TitlesJson);
        if (error is not null) return Failure(ProfileResultStatus.InvalidRequest, error);

        var secretary = await secretaryRepository.FindAsync(id, ct);
        if (secretary is null) return Failure(ProfileResultStatus.NotFound);

        secretary.TitlesJson = Clean(input.TitlesJson);
        await secretaryRepository.SaveChangesAsync(ct);

        return Success(secretary);
    }

    public async Task<ProfileResult<AcademicSecretaryResponseDto>> DeleteAsync(int id, CancellationToken ct)
    {
        var secretary = await secretaryRepository.FindAsync(id, ct);
        if (secretary is null) return Failure(ProfileResultStatus.NotFound);

        secretaryRepository.Remove(secretary);
        await secretaryRepository.SaveChangesAsync(ct);

        return Success(secretary);
    }

    private static string? ValidateTitles(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { using var _ = JsonDocument.Parse(json); return null; }
        catch (JsonException) { return "TitlesJson debe contener JSON válido."; }
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ProfileResult<AcademicSecretaryResponseDto> Success(AcademicSecretary s) =>
        new(ProfileResultStatus.Success, new AcademicSecretaryResponseDto(s.Id, s.UserId, s.TitlesJson));

    private static ProfileResult<AcademicSecretaryResponseDto> Failure(ProfileResultStatus status, string? message = null) =>
        new(status, Message: message);
}
