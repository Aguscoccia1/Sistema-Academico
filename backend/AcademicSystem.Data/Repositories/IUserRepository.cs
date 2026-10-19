using AcademicSystem.Entities.Models;

namespace AcademicSystem.Data.Repositories;

public interface IUserRepository
{
    User? GetActiveByEmail(string email);

    void SaveChanges();
}
