using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AcademicSystemContext _context;

    public UserRepository(AcademicSystemContext context)
    {
        _context = context;
    }

    public User? GetActiveByEmail(string email)
    {
        return _context.Users
            .Include(u => u.Role)
            .Include(u => u.Student)
            .Include(u => u.AcademicSecretary)
            .FirstOrDefault(u => u.Email == email && u.IsActive);
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public Task<List<UserResponseDto>> GetAllAsync(CancellationToken ct) =>
        _context.Users.AsNoTracking()
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Select(u => new UserResponseDto(u.Id, u.FirstName, u.LastName, u.Cuil, u.BirthDate, u.Address,
                u.RoleId, u.Role.Name, u.Email, u.Phone, u.EmergencyContactPhone, u.IsActive))
            .ToListAsync(ct);

    public Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        _context.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto(u.Id, u.FirstName, u.LastName, u.Cuil, u.BirthDate, u.Address,
                u.RoleId, u.Role.Name, u.Email, u.Phone, u.EmergencyContactPhone, u.IsActive))
            .SingleOrDefaultAsync(ct);

    public Task<User?> FindAsync(int id, CancellationToken ct) =>
        _context.Users.SingleOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> RoleExistsAsync(int roleId, CancellationToken ct) =>
        _context.Roles.AnyAsync(r => r.Id == roleId, ct);

    public Task<string?> GetRoleNameAsync(int roleId, CancellationToken ct) =>
        _context.Roles.Where(r => r.Id == roleId).Select(r => r.Name).SingleOrDefaultAsync(ct);

    public Task<string?> GetRoleNameOfUserAsync(int userId, CancellationToken ct) =>
        _context.Users.Where(u => u.Id == userId).Select(u => u.Role.Name).SingleOrDefaultAsync(ct);

    public Task<bool> CuilOrEmailExistsAsync(string cuil, string email, CancellationToken ct) =>
        _context.Users.AnyAsync(u => u.Cuil == cuil || u.Email == email, ct);

    public Task<bool> CuilOrEmailExistsAsync(string cuil, string email, int excludeId, CancellationToken ct) =>
        _context.Users.AnyAsync(u => u.Id != excludeId && (u.Cuil == cuil || u.Email == email), ct);

    public Task<bool> HasStudentProfileAsync(int userId, CancellationToken ct) =>
        _context.Students.AnyAsync(s => s.UserId == userId, ct);

    public Task<bool> HasSecretaryProfileAsync(int userId, CancellationToken ct) =>
        _context.AcademicSecretaries.AnyAsync(s => s.UserId == userId, ct);

    public Task<bool> IsActiveWithRoleAsync(int userId, string roleName, CancellationToken ct) =>
        _context.Users.AnyAsync(u => u.Id == userId && u.IsActive && u.Role.Name == roleName, ct);

    public Task<bool> OtherActiveWithRoleExistsAsync(int userId, string roleName, CancellationToken ct) =>
        _context.Users.AnyAsync(u => u.Id != userId && u.IsActive && u.Role.Name == roleName, ct);

    public void Add(User user) => _context.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
