using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Entities.DTOs;

/// <summary>One configured weekly schedule for a subject commission.</summary>
public sealed record HorarioMateriaResponseDto(
    int Id,
    int SubjectId,
    string Section,
    string Day,
    TimeOnly StartTime,
    TimeOnly EndTime);

/// <summary>Data required to configure a weekly schedule for a subject commission.</summary>
public sealed record CrearHorarioMateriaDto
{
    [Range(1, int.MaxValue)]
    public int SubjectId { get; init; }

    [Required, RegularExpression("^[AB]$")]
    public required string Section { get; init; }

    [Required, RegularExpression("^(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)$")]
    public required string Day { get; init; }

    [Required]
    public TimeOnly? StartTime { get; init; }

    [Required]
    public TimeOnly? EndTime { get; init; }
}
