
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public interface IComisionesService
{
    Task<IReadOnlyList<ComisionResponseDto>> ObtenerTodasAsync(
        int? materiaId,
        CancellationToken cancellationToken = default);

    Task<EstudianteResponseDto?> BuscarEstudianteAsync(
        int materiaId,
        string seccion,
        int estudianteId,
        CancellationToken cancellationToken = default);
}

public class ComisionesService(AcademicSystemContext context) : IComisionesService
{
    public async Task<IReadOnlyList<ComisionResponseDto>> ObtenerTodasAsync(
        int? materiaId,
        CancellationToken cancellationToken = default)
    {
        var comisionesDocentes = context.TeacherSubjects
            .Where(ts => materiaId == null || ts.SubjectId == materiaId)
            .Select(ts => new
            {
                ts.SubjectId,
                Materia = ts.Subject.Name,
                Seccion = ts.Section
            });

        var comisionesConInscriptos = context.StudentSubjects
            .Where(ss => materiaId == null || ss.SubjectId == materiaId)
            .Select(ss => new
            {
                ss.SubjectId,
                Materia = ss.Subject.Name,
                Seccion = ss.Section
            });

        return await comisionesDocentes
            .Union(comisionesConInscriptos)
            .Distinct()
            .Select(comision => new ComisionResponseDto(
                comision.SubjectId,
                comision.Materia,
                comision.Seccion,
                context.StudentSubjects.Count(ss =>
                    ss.SubjectId == comision.SubjectId &&
                    ss.Section == comision.Seccion)))
            .OrderBy(comision => comision.MateriaId)
            .ThenBy(comision => comision.Seccion)
            .ToListAsync(cancellationToken);
    }

    public async Task<EstudianteResponseDto?> BuscarEstudianteAsync(
        int materiaId,
        string seccion,
        int estudianteId,
        CancellationToken cancellationToken = default)
    {
        return await context.StudentSubjects
            .Where(ss =>
                ss.SubjectId == materiaId &&
                ss.Section == seccion &&
                ss.StudentId == estudianteId)
            .Select(ss => new EstudianteResponseDto(
                ss.Student.Id,
                ss.Student.FirstName,
                ss.Student.LastName,
                ss.Student.Dni,
                ss.Student.RecordNumber))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
