//(damian)
using AcademicSystem.Business.Services;
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace AcademicSystem.Api;

public static class DevelopmentSeeder
{
    public static void SeedLoginUsers(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        AcademicSystemContext context = scope.ServiceProvider.GetRequiredService<AcademicSystemContext>();
        PasswordHasher<User> hasher = new PasswordHasher<User>();

        if (!context.Users.Any(u => u.Email == "loko@uni.edu"))
        {
            Role studentRole = context.Roles.First(r => r.Name == RoleNames.Student);

            User studentUser = new User
            {
                FirstName = "Jeremias",
                LastName = "Perez",
                Cuil = "27300000001",
                Email = "loko@uni.edu",
                RoleId = studentRole.Id,
                IsActive = true,
                PasswordHash = ""
            };
            studentUser.PasswordHash = hasher.HashPassword(studentUser, "1234");
            studentUser.Student = new Student
            {
                FirstName = "Jeremias",
                LastName = "Perez",
                Dni = "30000000",
                RecordNumber = "L-1001"
            };

            context.Users.Add(studentUser);
        }

        if (!context.Users.Any(u => u.Email == "maria@uni.edu"))
        {
            Role secretaryRole = context.Roles.First(r => r.Name == RoleNames.AcademicSecretary);

            User secretaryUser = new User
            {
                FirstName = "Maria",
                LastName = "Agui",
                Cuil = "27300000002",
                Email = "maria@uni.edu",
                RoleId = secretaryRole.Id,
                IsActive = true,
                PasswordHash = ""
            };
            secretaryUser.PasswordHash = hasher.HashPassword(secretaryUser, "admin");
            secretaryUser.AcademicSecretary = new AcademicSecretary();

            context.Users.Add(secretaryUser);
        }

        context.SaveChanges();
    }
}