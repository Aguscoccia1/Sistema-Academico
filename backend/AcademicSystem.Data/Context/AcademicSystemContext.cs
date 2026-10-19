using System;
using System.Collections.Generic;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Context;

public partial class AcademicSystemContext : DbContext
{
    public AcademicSystemContext(DbContextOptions<AcademicSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AbsenceJustification> AbsenceJustifications { get; set; }

    public virtual DbSet<AcademicSecretary> AcademicSecretaries { get; set; }

    public virtual DbSet<AssessmentInstance> AssessmentInstances { get; set; }

    public virtual DbSet<CourseRecord> CourseRecords { get; set; }

    public virtual DbSet<Director> Directors { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<PartialGrade> PartialGrades { get; set; }

    public virtual DbSet<Prerequisite> Prerequisites { get; set; }

    public virtual DbSet<ProgramContent> ProgramContents { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleDocumentType> RoleDocumentTypes { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentSubject> StudentSubjects { get; set; }

    public virtual DbSet<StudyPlan> StudyPlans { get; set; }

    public virtual DbSet<StudyPlanSubject> StudyPlanSubjects { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<SubjectProgram> SubjectPrograms { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<TeacherSubject> TeacherSubjects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDocument> UserDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AbsenceJustification>(entity =>
        {
            entity.HasIndex(e => e.AuditorUserId, "IX_AbsenceJustifications_AuditorUserId");

            entity.HasIndex(e => e.UserId, "IX_AbsenceJustifications_UserId");

            entity.Property(e => e.AbsenceType).HasMaxLength(50);
            entity.Property(e => e.AdditionalNote).HasMaxLength(500);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UploadedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_AbsenceJustifications_UploadedAt");

            entity.HasOne(d => d.AuditorUser).WithMany(p => p.AbsenceJustificationAuditorUsers)
                .HasForeignKey(d => d.AuditorUserId)
                .HasConstraintName("FK_AbsenceJustifications_AuditorUsers");

            entity.HasOne(d => d.User).WithMany(p => p.AbsenceJustificationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AbsenceJustifications_Users");
        });

        modelBuilder.Entity<AcademicSecretary>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_AcademicSecretaries_UserId").IsUnique();

            entity.HasOne(d => d.User).WithOne(p => p.AcademicSecretary)
                .HasForeignKey<AcademicSecretary>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AcademicSecretaries_Users");
        });

        modelBuilder.Entity<AssessmentInstance>(entity =>
        {
            entity.HasIndex(e => e.RetakeOfInstanceId, "IX_AssessmentInstances_RetakeOfInstanceId");

            entity.HasIndex(e => e.StudyPlanSubjectId, "IX_AssessmentInstances_StudyPlanSubjectId");

            entity.Property(e => e.InstanceDate).HasPrecision(0);
            entity.Property(e => e.InstanceType).HasMaxLength(30);

            entity.HasOne(d => d.RetakeOfInstance).WithMany(p => p.InverseRetakeOfInstance)
                .HasForeignKey(d => d.RetakeOfInstanceId)
                .HasConstraintName("FK_AssessmentInstances_RetakeOf");

            entity.HasOne(d => d.StudyPlanSubject).WithMany(p => p.AssessmentInstances)
                .HasForeignKey(d => d.StudyPlanSubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssessmentInstances_StudyPlanSubjects");
        });

        modelBuilder.Entity<CourseRecord>(entity =>
        {
            entity.HasIndex(e => e.StudentSubjectId, "UQ_CourseRecords_StudentSubjectId").IsUnique();

            entity.Property(e => e.AttendancePercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FinalStatus).HasMaxLength(30);
            entity.Property(e => e.LetterGrade).HasMaxLength(20);
            entity.Property(e => e.NumericGrade).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.Observations).HasMaxLength(1000);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_CourseRecords_UpdatedAt");

            entity.HasOne(d => d.StudentSubject).WithOne(p => p.CourseRecord)
                .HasForeignKey<CourseRecord>(d => d.StudentSubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseRecords_StudentSubjects");
        });

        modelBuilder.Entity<Director>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_Directors_UserId").IsUnique();

            entity.HasOne(d => d.User).WithOne(p => p.Director)
                .HasForeignKey<Director>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Directors_Users");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_DocumentTypes_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasIndex(e => e.SubjectId, "IX_Exams_SubjectId");

            entity.HasIndex(e => e.TeacherId, "IX_Exams_TeacherId");

            entity.Property(e => e.ExamDate).HasPrecision(0);
            entity.Property(e => e.ExamType).HasMaxLength(30);

            entity.HasOne(d => d.Subject).WithMany(p => p.Exams)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exams_Subjects");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Exams)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exams_Teachers");
        });

        modelBuilder.Entity<PartialGrade>(entity =>
        {
            entity.HasIndex(e => e.AssessmentInstanceId, "IX_PartialGrades_AssessmentInstanceId");

            entity.HasIndex(e => e.RecordedByTeacherId, "IX_PartialGrades_RecordedByTeacherId");

            entity.HasIndex(e => new { e.StudentSubjectId, e.AssessmentInstanceId }, "UQ_PartialGrades").IsUnique();

            entity.Property(e => e.Grade).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.RecordedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_PartialGrades_RecordedAt");

            entity.HasOne(d => d.AssessmentInstance).WithMany(p => p.PartialGrades)
                .HasForeignKey(d => d.AssessmentInstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartialGrades_AssessmentInstances");

            entity.HasOne(d => d.RecordedByTeacher).WithMany(p => p.PartialGrades)
                .HasForeignKey(d => d.RecordedByTeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartialGrades_Teachers");

            entity.HasOne(d => d.StudentSubject).WithMany(p => p.PartialGrades)
                .HasForeignKey(d => d.StudentSubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartialGrades_StudentSubjects");
        });

        modelBuilder.Entity<Prerequisite>(entity =>
        {
            entity.HasIndex(e => e.PrerequisiteSubjectId, "IX_Prerequisites_PrerequisiteSubjectId");

            entity.HasIndex(e => e.SubjectId, "IX_Prerequisites_SubjectId");

            entity.HasIndex(e => new { e.StudyPlanId, e.SubjectId, e.PrerequisiteSubjectId }, "UQ_Prerequisites").IsUnique();

            entity.HasOne(d => d.PrerequisiteSubject).WithMany(p => p.PrerequisitePrerequisiteSubjects)
                .HasForeignKey(d => d.PrerequisiteSubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prerequisites_PrerequisiteSubjects");

            entity.HasOne(d => d.StudyPlan).WithMany(p => p.Prerequisites)
                .HasForeignKey(d => d.StudyPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prerequisites_StudyPlans");

            entity.HasOne(d => d.Subject).WithMany(p => p.PrerequisiteSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prerequisites_Subjects");
        });

        modelBuilder.Entity<ProgramContent>(entity =>
        {
            entity.HasIndex(e => new { e.ProgramId, e.UnitNumber }, "UQ_ProgramContents").IsUnique();

            entity.Property(e => e.UnitTitle).HasMaxLength(200);

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramContents)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgramContents_SubjectPrograms");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<RoleDocumentType>(entity =>
        {
            entity.HasIndex(e => e.DocumentTypeId, "IX_RoleDocumentTypes_DocumentTypeId");

            entity.HasIndex(e => new { e.RoleId, e.DocumentTypeId }, "UQ_RoleDocumentTypes").IsUnique();

            entity.HasOne(d => d.DocumentType).WithMany(p => p.RoleDocumentTypes)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleDocumentTypes_DocumentTypes");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleDocumentTypes)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleDocumentTypes_Roles");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.Dni, "UQ_Students_Dni").IsUnique();

            entity.HasIndex(e => e.RecordNumber, "UQ_Students_RecordNumber").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_Students_UserId").IsUnique();

            entity.Property(e => e.Dni)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.RecordNumber).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<StudentSubject>(entity =>
        {
            entity.HasIndex(e => e.SubjectId, "IX_StudentSubjects_SubjectId");

            entity.HasIndex(e => new { e.StudentId, e.SubjectId }, "UQ_StudentSubjects").IsUnique();

            entity.Property(e => e.Section).HasMaxLength(20);

            entity.HasOne(d => d.Student).WithMany(p => p.StudentSubjects)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentSubjects_Students");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudentSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentSubjects_Subjects");
        });

        modelBuilder.Entity<StudyPlan>(entity =>
        {
            entity.HasIndex(e => e.PlanCode, "UQ_StudyPlans_PlanCode").IsUnique();

            entity.Property(e => e.AttendanceMode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_StudyPlans_IsActive");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PlanCode).HasMaxLength(30);
        });

        modelBuilder.Entity<StudyPlanSubject>(entity =>
        {
            entity.HasIndex(e => e.SubjectId, "IX_StudyPlanSubjects_SubjectId");

            entity.HasIndex(e => new { e.StudyPlanId, e.SubjectId }, "UQ_StudyPlanSubjects").IsUnique();

            entity.HasOne(d => d.StudyPlan).WithMany(p => p.StudyPlanSubjects)
                .HasForeignKey(d => d.StudyPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudyPlanSubjects_StudyPlans");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudyPlanSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudyPlanSubjects_Subjects");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(e => e.Course).HasMaxLength(50);
            entity.Property(e => e.Format).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<SubjectProgram>(entity =>
        {
            entity.HasIndex(e => e.TeacherId, "IX_SubjectPrograms_TeacherId");

            entity.HasIndex(e => new { e.SubjectId, e.TeacherId, e.AcademicYear }, "UQ_SubjectPrograms").IsUnique();

            entity.Property(e => e.CurricularFormat).HasMaxLength(50);
            entity.Property(e => e.SubjectCondition).HasMaxLength(50);
            entity.Property(e => e.TermHours).HasMaxLength(50);
            entity.Property(e => e.WeeklyHours).HasMaxLength(50);

            entity.HasOne(d => d.Subject).WithMany(p => p.SubjectPrograms)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubjectPrograms_Subjects");

            entity.HasOne(d => d.Teacher).WithMany(p => p.SubjectPrograms)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubjectPrograms_Teachers");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_Teachers_UserId").IsUnique();

            entity.HasOne(d => d.User).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Teachers_Users");
        });

        modelBuilder.Entity<TeacherSubject>(entity =>
        {
            entity.HasIndex(e => e.SubjectId, "IX_TeacherSubjects_SubjectId");

            entity.HasIndex(e => new { e.TeacherId, e.SubjectId, e.Section }, "UQ_TeacherSubjects").IsUnique();

            entity.Property(e => e.Section).HasMaxLength(20);

            entity.HasOne(d => d.Subject).WithMany(p => p.TeacherSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeacherSubjects_Subjects");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherSubjects)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeacherSubjects_Teachers");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");

            entity.HasIndex(e => e.Cuil, "UQ_Users_Cuil").IsUnique();

            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Cuil)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.FailedLoginWindowStartedAt).HasColumnType("datetimeoffset");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Users_IsActive");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.LockedUntil).HasColumnType("datetimeoffset");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<UserDocument>(entity =>
        {
            entity.HasIndex(e => e.DocumentTypeId, "IX_UserDocuments_DocumentTypeId");

            entity.HasIndex(e => e.UserId, "IX_UserDocuments_UserId");

            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.ExpiresAt).HasPrecision(0);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UploadedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_UserDocuments_UploadedAt");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.UserDocuments)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserDocuments_DocumentTypes");

            entity.HasOne(d => d.User).WithMany(p => p.UserDocuments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserDocuments_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
