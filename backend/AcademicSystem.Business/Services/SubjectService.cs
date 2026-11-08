using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using AcademicSystem.Data.Repositories;

namespace AcademicSystem.Business.Services;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;
 
    public SubjectService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<List<SubjectResponseDto>> GetAllAsync(CancellationToken ct)
    {
        List<Subject> subjects = await _subjectRepository.GetAllAsync(ct);
 
        return subjects
            .Select(ToResponse)
            .ToList();
    }
 
    public async Task<SubjectResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        Subject? subject = await _subjectRepository.GetByIdAsync(id, ct);
 
        return subject == null ? null : ToResponse(subject);
    }
 
    public async Task<SubjectResponseDto> CreateAsync(SubjectRequestDto request, CancellationToken ct)
    {
        Subject subject = new Subject();
        CopyFields(request, subject);
 
        _subjectRepository.Add(subject);
        await _subjectRepository.SaveChangesAsync(ct);
 
        return ToResponse(subject);
    }
 
    public async Task<SubjectResponseDto?> UpdateAsync(int id, SubjectRequestDto request, CancellationToken ct)
    {
        Subject? subject = await _subjectRepository.GetByIdAsync(id, ct);
 
        if (subject == null)
        {
            return null;
        }
 
        CopyFields(request, subject);
        await _subjectRepository.SaveChangesAsync(ct);
 
        return ToResponse(subject);
    }
 
    public static SubjectResponseDto ToResponse(Subject subject)
    {
        return new SubjectResponseDto
        {
            Id = subject.Id,
            Name = subject.Name,
            OrderNumber = subject.OrderNumber,
            Format = subject.Format,
            LectureHours = subject.LectureHours,
            TotalHours = subject.TotalHours,
            Course = subject.Course
        };
    }
 
    private static void CopyFields(SubjectRequestDto source, Subject target)
    {
        target.Name = source.Name;
        target.OrderNumber = source.OrderNumber;
        target.Format = source.Format;
        target.LectureHours = source.LectureHours;
        target.TotalHours = source.TotalHours;
        target.Course = source.Course;
    }
}