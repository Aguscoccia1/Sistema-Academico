using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Business.Services;

public class StudentProfileService(IStudentProfileRepository studentRepository, IUserRepository userRepository)
    : IStudentProfileService
{
    public Task<List<StudentProfileResponseDto>> GetAllAsync(CancellationToken ct) => studentRepository.GetAllAsync(ct);

    public Task<StudentProfileResponseDto?> GetByIdAsync(int id, CancellationToken ct) => studentRepository.GetByIdAsync(id, ct);

    public async Task<ProfileResult<StudentProfileResponseDto>> CreateAsync(StudentProfileRequestDto input, CancellationToken ct)
    {
        var error = Validate(input);
        if (error is not null) return Failure(ProfileResultStatus.InvalidRequest, error);

        var userRole = await userRepository.GetRoleNameOfUserAsync(input.UserId, ct);
        if (userRole is null) return Failure(ProfileResultStatus.InvalidRequest, "El usuario indicado no existe.");
        if (userRole != RoleNames.Student)
            return Failure(ProfileResultStatus.InvalidRequest, "El usuario debe tener el rol Student para crear un perfil de estudiante.");

        var dni = input.Dni!.Trim();
        var legajo = input.RecordNumber!.Trim();
        if (await studentRepository.UserOrDocumentExistsAsync(input.UserId, dni, legajo, ct))
            return Failure(ProfileResultStatus.Conflict, "El usuario ya tiene perfil o el DNI/número de legajo ya existe.");

        var student = new Student
        {
            UserId = input.UserId,
            FirstName = input.FirstName!.Trim(),
            LastName = input.LastName!.Trim(),
            Dni = dni,
            RecordNumber = legajo
        };

        studentRepository.Add(student);
        await studentRepository.SaveChangesAsync(ct);

        return Success(student);
    }

    public async Task<ProfileResult<StudentProfileResponseDto>> UpdateAsync(int id, StudentProfileRequestDto input, CancellationToken ct)
    {
        var error = Validate(input);
        if (error is not null) return Failure(ProfileResultStatus.InvalidRequest, error);

        var student = await studentRepository.FindAsync(id, ct);
        if (student is null) return Failure(ProfileResultStatus.NotFound);

        var dni = input.Dni!.Trim();
        var legajo = input.RecordNumber!.Trim();
        if (await studentRepository.DocumentExistsForOtherAsync(id, dni, legajo, ct))
            return Failure(ProfileResultStatus.Conflict, "Otro estudiante ya tiene ese DNI o número de legajo.");

        student.FirstName = input.FirstName!.Trim();
        student.LastName = input.LastName!.Trim();
        student.Dni = dni;
        student.RecordNumber = legajo;
        await studentRepository.SaveChangesAsync(ct);

        return Success(student);
    }

    public async Task<ProfileResult<StudentProfileResponseDto>> DeleteAsync(int id, CancellationToken ct)
    {
        var student = await studentRepository.FindAsync(id, ct);
        if (student is null) return Failure(ProfileResultStatus.NotFound);

        if (await studentRepository.HasAcademicHistoryAsync(id, ct))
            return Failure(ProfileResultStatus.Conflict, "No se puede eliminar el estudiante porque tiene historial académico asociado.");

        studentRepository.Remove(student);
        await studentRepository.SaveChangesAsync(ct);

        return Success(student);
    }

    private static string? Validate(StudentProfileRequestDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName)) return "Nombre y apellido son obligatorios.";
        var cleanDni = input.Dni?.Trim() ?? "";
        if (cleanDni.Length is < 7 or > 8 || cleanDni.Any(c => !char.IsDigit(c))) return "El DNI debe contener 7 u 8 dígitos.";
        if (string.IsNullOrWhiteSpace(input.RecordNumber) || input.RecordNumber.Trim().Length > 20) return "El número de legajo es obligatorio y admite hasta 20 caracteres.";
        return null;
    }

    private static ProfileResult<StudentProfileResponseDto> Success(Student s) =>
        new(ProfileResultStatus.Success, new StudentProfileResponseDto(s.Id, s.UserId, s.FirstName, s.LastName, s.Dni, s.RecordNumber));

    private static ProfileResult<StudentProfileResponseDto> Failure(ProfileResultStatus status, string? message = null) =>
        new(status, Message: message);
}
