//(damian)
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Business.Services;

public interface IAuthService
{
    User? Login(string email, string password);
}