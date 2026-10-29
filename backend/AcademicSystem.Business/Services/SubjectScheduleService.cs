using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public enum EstadoConsultaHorarios
{
    Encontrado,
    MateriaInexistente
}

public sealed record ResultadoConsultaHorarios(
    EstadoConsultaHorarios Estado,
    IReadOnlyList<HorarioMateriaResponseDto> Horarios);

public enum EstadoCreacionHorario
{
    Creado,
    MateriaInexistente,
    ComisionInexistente,
    HorarioSuperpuesto
}

public sealed record ResultadoCreacionHorario(
    EstadoCreacionHorario Estado,
    HorarioMateriaResponseDto? Horario);

public interface ISubjectScheduleService
{
    Task<ResultadoConsultaHorarios> ObtenerAsync(
        int subjectId,
        string section,
        CancellationToken cancellationToken = default);

    Task<ResultadoCreacionHorario> CrearAsync(
        CrearHorarioMateriaDto solicitud,
        CancellationToken cancellationToken = default);
}

public sealed class SubjectScheduleService(AcademicSystemContext context) : ISubjectScheduleService
{
    public async Task<ResultadoConsultaHorarios> ObtenerAsync(
        int subjectId,
        string section,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Subjects.AnyAsync(
                subject => subject.Id == subjectId,
                cancellationToken))
        {
            return new ResultadoConsultaHorarios(
                EstadoConsultaHorarios.MateriaInexistente,
                []);
        }

        var horarios = await context.SubjectSchedules
            .AsNoTracking()
            .Where(schedule =>
                schedule.SubjectId == subjectId &&
                schedule.Section == section)
            .Select(schedule => new HorarioMateriaResponseDto(
                schedule.Id,
                schedule.SubjectId,
                schedule.Section,
                schedule.Day,
                schedule.StartTime,
                schedule.EndTime))
            .ToListAsync(cancellationToken);

        var ordenDias = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Monday"] = 0,
            ["Tuesday"] = 1,
            ["Wednesday"] = 2,
            ["Thursday"] = 3,
            ["Friday"] = 4,
            ["Saturday"] = 5,
            ["Sunday"] = 6
        };

        return new ResultadoConsultaHorarios(
            EstadoConsultaHorarios.Encontrado,
            horarios
                .OrderBy(schedule => ordenDias.GetValueOrDefault(schedule.Day, int.MaxValue))
                .ThenBy(schedule => schedule.StartTime)
                .ToList());
    }

    public async Task<ResultadoCreacionHorario> CrearAsync(
        CrearHorarioMateriaDto solicitud,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Subjects.AnyAsync(
                subject => subject.Id == solicitud.SubjectId,
                cancellationToken))
        {
            return new ResultadoCreacionHorario(
                EstadoCreacionHorario.MateriaInexistente,
                null);
        }

        var startTime = solicitud.StartTime!.Value;
        var endTime = solicitud.EndTime!.Value;
        var horarioSuperpuesto = await context.SubjectSchedules.AnyAsync(
            schedule =>
                schedule.SubjectId == solicitud.SubjectId &&
                schedule.Section == solicitud.Section &&
                schedule.Day == solicitud.Day &&
                schedule.StartTime < endTime &&
                schedule.EndTime > startTime,
            cancellationToken);

        if (horarioSuperpuesto)
        {
            return new ResultadoCreacionHorario(
                EstadoCreacionHorario.HorarioSuperpuesto,
                null);
        }

        var horario = new SubjectSchedule
        {
            SubjectId = solicitud.SubjectId,
            Section = solicitud.Section,
            Day = solicitud.Day,
            StartTime = startTime,
            EndTime = endTime
        };
        context.SubjectSchedules.Add(horario);

        await context.SaveChangesAsync(cancellationToken);
        return new ResultadoCreacionHorario(
            EstadoCreacionHorario.Creado,
            new HorarioMateriaResponseDto(
                horario.Id,
                horario.SubjectId,
                horario.Section,
                horario.Day,
                horario.StartTime,
                horario.EndTime));
    }
}
