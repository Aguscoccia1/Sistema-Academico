using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using AcademicSystem.Data.Context;

namespace AcademicSystem.Business.Services;

public class SubjectService : ISubjectService
{
    private readonly AcademicSystemContext _context;

    public SubjectService(AcademicSystemContext context)
    {
        _context = context;
    }

    public List<SubjectResponseDto> GetAll()
    {
        return _context.Subjects
            .Select(s => new SubjectResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                OrderNumber = s.OrderNumber,
                Format = s.Format,
                LectureHours = s.LectureHours,
                TotalHours = s.TotalHours,
                Course = s.Course
            })
            .ToList();
    }

    public Subject? GetById(int id)
    {
        return _context.Subjects.FirstOrDefault(s => s.Id == id);
    }

    public Subject Create(Subject subject)
    {
        _context.Subjects.Add(subject);
        _context.SaveChanges();
        return subject;
    }

    public Subject? Update(int id, Subject changes)
    {
        Subject? existing = _context.Subjects.FirstOrDefault(s => s.Id == id);

        if (existing == null)
        {
            return null;
        }

        existing.Name = changes.Name;
        existing.OrderNumber = changes.OrderNumber;
        existing.Format = changes.Format;
        existing.LectureHours = changes.LectureHours;
        existing.TotalHours = changes.TotalHours;
        existing.Course = changes.Course;

        _context.SaveChanges();
        return existing;
    }
}