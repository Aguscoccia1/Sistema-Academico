using System;
using System.Collections.Generic;

namespace AcademicSystem.Entities.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Cuil { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public string? Address { get; set; }

    public int RoleId { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTimeOffset? FailedLoginWindowStartedAt { get; set; }

    public DateTimeOffset? LockedUntil { get; set; }

    public virtual ICollection<AbsenceJustification> AbsenceJustificationAuditorUsers { get; set; } = new List<AbsenceJustification>();

    public virtual ICollection<AbsenceJustification> AbsenceJustificationUsers { get; set; } = new List<AbsenceJustification>();

    public virtual AcademicSecretary? AcademicSecretary { get; set; }

    public virtual Director? Director { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }

    public virtual ICollection<UserDocument> UserDocuments { get; set; } = new List<UserDocument>();
}
