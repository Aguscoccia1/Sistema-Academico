using AcademicSystem.Data.Context;
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
}
