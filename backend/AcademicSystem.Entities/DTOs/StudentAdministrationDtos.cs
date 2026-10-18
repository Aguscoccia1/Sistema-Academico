using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Entities.DTOs;

public sealed record EstudianteResponseDto(
    int Id,
    string Nombre,
    string Apellido,
    string Dni,
    string Legajo);

public sealed record MateriaResponseDto(
    int Id,
    string Nombre,
    int Anio);

public sealed record InscripcionResponseDto(
    int Id,
    int EstudianteId,
    int MateriaId,
    string Materia,
    string Seccion);

public sealed record ComisionResponseDto(
    int MateriaId,
    string Materia,
    string Seccion,
    int CantidadEstudiantes);

public sealed record AsignacionAleatoriaMasivaRequestDto
{
    [Range(1, int.MaxValue)]
    public int MateriaId { get; init; }
}

public sealed record DistribucionComisionDto(
    string Seccion,
    int CantidadAsignada);

public sealed record AsignacionAleatoriaMasivaResponseDto(
    bool Exito,
    string Mensaje,
    int TotalAsignado,
    IReadOnlyList<DistribucionComisionDto> Distribucion);

public sealed record ResultadoInscripcionDto(bool Exito, string Mensaje);

public sealed record ModificarEstudianteDto
{
    [Required, MaxLength(100)]
    public required string Nombre { get; init; }

    [Required, MaxLength(100)]
    public required string Apellido { get; init; }

    [Required, RegularExpression(@"^\d{7,8}$")]
    public required string Dni { get; init; }
}

public sealed record ReasignarComisionDto
{
    [Range(1, int.MaxValue)]
    public int MateriaId { get; init; }

    [Required, MaxLength(20)]
    public required string Seccion { get; init; }
}

public sealed record CrearInscripcionDto
{
    [Range(1, int.MaxValue)]
    public int EstudianteId { get; init; }

    [Range(1, int.MaxValue)]
    public int MateriaId { get; init; }

    [Required, MaxLength(20)]
    public required string Seccion { get; init; }
}

public enum EstadoModificacionEstudiante
{
    Actualizado,
    NoEncontrado,
    DniDuplicado
}

public enum EstadoReasignacionComision
{
    Reasignada,
    InscripcionNoEncontrada,
    ComisionInexistente,
    YaAsignada
}
