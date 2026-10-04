/* =============================================================================
   AcademicSystem - database schema
   Instituto Superior Cura Gabriel Brochero

   Built from the UML class diagram (diagrama_uml_Sistema_Academico.md).
   Target: SQL Server 2016 or later.

   How to run it:
     - SQL Server Management Studio: open this file and press F5.
     - Command line:  sqlcmd -S ALAN -E -C -f 65001 -i AcademicSystem.sql

   It is safe to run more than once: it only creates what is missing.

   UML class (Spanish)        Table (English)
   -------------------------  ------------------------
   Rol                        Roles
   Usuario                    Users
   Director                   Directors
   SecretarioAcademico        AcademicSecretaries
   Docente                    Teachers
   Alumno                     Students
   Materia                    Subjects
   Docente_Materia            TeacherSubjects
   Alumno_Materia             StudentSubjects
   Examen                     Exams
   Programa_Materia           SubjectPrograms
   Contenido                  ProgramContents
   Tipo_Documento             DocumentTypes
   Roles_Tipos_Documentos     RoleDocumentTypes
   Legajo                     UserDocuments
   Justificativo              AbsenceJustifications
   PlanEstudio                StudyPlans
   PlanMateria                StudyPlanSubjects
   Correlatividad             Prerequisites
   InstanciaParcial           AssessmentInstances
   NotaParcial                PartialGrades
   RegistroCursada            CourseRecords

   Conventions
     - Table names in plural, columns in PascalCase.
     - Every table has an "Id" primary key (INT IDENTITY).
     - Foreign keys are named <Entity>Id, for example RoleId or SubjectId.
     - No cascading deletes: records are deactivated, not deleted.
   ============================================================================= */

USE master;
GO

IF DB_ID(N'AcademicSystem') IS NULL
    CREATE DATABASE AcademicSystem;
GO

USE AcademicSystem;
GO

/* =============================================================================
   1. Users and roles
   ============================================================================= */

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
CREATE TABLE dbo.Roles (
    Id    INT IDENTITY(1,1) NOT NULL,
    Name  NVARCHAR(50)      NOT NULL,

    CONSTRAINT PK_Roles      PRIMARY KEY (Id),
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    Id                     INT IDENTITY(1,1) NOT NULL,
    FirstName              NVARCHAR(100)     NOT NULL,
    LastName               NVARCHAR(100)     NOT NULL,
    Cuil                   CHAR(11)          NOT NULL,   -- 11 digits, no dashes
    BirthDate              DATE              NULL,
    Address                NVARCHAR(200)     NULL,
    RoleId                 INT               NOT NULL,
    Email                  NVARCHAR(255)     NOT NULL,
    Phone                  NVARCHAR(20)      NULL,
    EmergencyContactPhone  NVARCHAR(20)      NULL,
    PasswordHash           NVARCHAR(255)     NOT NULL,
    IsActive               BIT               NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),

    CONSTRAINT PK_Users            PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Cuil       UNIQUE (Cuil),
    CONSTRAINT UQ_Users_Email      UNIQUE (Email),
    CONSTRAINT CK_Users_Cuil       CHECK (Cuil NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_Users_BirthDate  CHECK (BirthDate IS NULL OR BirthDate <= CAST(GETDATE() AS DATE)),
    CONSTRAINT FK_Users_Roles      FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id),
    INDEX IX_Users_RoleId (RoleId)
);
GO

/* =============================================================================
   2. Profiles: each one is linked to exactly one user
   ============================================================================= */

IF OBJECT_ID(N'dbo.Directors', N'U') IS NULL
CREATE TABLE dbo.Directors (
    Id          INT IDENTITY(1,1) NOT NULL,
    UserId      INT               NOT NULL,
    TitlesJson  NVARCHAR(MAX)     NULL,

    CONSTRAINT PK_Directors             PRIMARY KEY (Id),
    CONSTRAINT UQ_Directors_UserId      UNIQUE (UserId),
    CONSTRAINT CK_Directors_TitlesJson  CHECK (TitlesJson IS NULL OR ISJSON(TitlesJson) = 1),
    CONSTRAINT FK_Directors_Users       FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
);
GO

IF OBJECT_ID(N'dbo.AcademicSecretaries', N'U') IS NULL
CREATE TABLE dbo.AcademicSecretaries (
    Id          INT IDENTITY(1,1) NOT NULL,
    UserId      INT               NOT NULL,
    TitlesJson  NVARCHAR(MAX)     NULL,

    CONSTRAINT PK_AcademicSecretaries             PRIMARY KEY (Id),
    CONSTRAINT UQ_AcademicSecretaries_UserId      UNIQUE (UserId),
    CONSTRAINT CK_AcademicSecretaries_TitlesJson  CHECK (TitlesJson IS NULL OR ISJSON(TitlesJson) = 1),
    CONSTRAINT FK_AcademicSecretaries_Users       FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
);
GO

IF OBJECT_ID(N'dbo.Teachers', N'U') IS NULL
CREATE TABLE dbo.Teachers (
    Id                    INT IDENTITY(1,1) NOT NULL,
    UserId                INT               NOT NULL,
    TitlesJson            NVARCHAR(MAX)     NULL,
    IsSubstituteDirector  BIT               NOT NULL CONSTRAINT DF_Teachers_IsSubstituteDirector DEFAULT (0),

    CONSTRAINT PK_Teachers             PRIMARY KEY (Id),
    CONSTRAINT UQ_Teachers_UserId      UNIQUE (UserId),
    CONSTRAINT CK_Teachers_TitlesJson  CHECK (TitlesJson IS NULL OR ISJSON(TitlesJson) = 1),
    CONSTRAINT FK_Teachers_Users       FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
);
GO

IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
CREATE TABLE dbo.Students (
    Id            INT IDENTITY(1,1) NOT NULL,
    UserId        INT               NOT NULL,
    FirstName     NVARCHAR(100)     NOT NULL,
    LastName      NVARCHAR(100)     NOT NULL,
    Dni           VARCHAR(10)       NOT NULL,   -- digits only, no dots
    RecordNumber  NVARCHAR(20)      NOT NULL,   -- "legajo" number

    CONSTRAINT PK_Students               PRIMARY KEY (Id),
    CONSTRAINT UQ_Students_UserId        UNIQUE (UserId),
    CONSTRAINT UQ_Students_Dni           UNIQUE (Dni),
    CONSTRAINT UQ_Students_RecordNumber  UNIQUE (RecordNumber),
    CONSTRAINT CK_Students_Dni           CHECK (Dni NOT LIKE '%[^0-9]%' AND LEN(Dni) BETWEEN 7 AND 8),
    CONSTRAINT FK_Students_Users         FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
);
GO

/* =============================================================================
   3. Subjects, assignments and enrollments
   ============================================================================= */

IF OBJECT_ID(N'dbo.Subjects', N'U') IS NULL
CREATE TABLE dbo.Subjects (
    Id            INT IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(150)     NOT NULL,
    OrderNumber   INT               NULL,
    Format        NVARCHAR(50)      NULL,   -- curricular format: subject, workshop, seminar...
    LectureHours  INT               NULL,   -- "horas catedra"
    TotalHours    INT               NULL,
    Course        NVARCHAR(50)      NULL,

    CONSTRAINT PK_Subjects               PRIMARY KEY (Id),
    CONSTRAINT CK_Subjects_OrderNumber   CHECK (OrderNumber IS NULL OR OrderNumber > 0),
    CONSTRAINT CK_Subjects_LectureHours  CHECK (LectureHours IS NULL OR LectureHours >= 0),
    CONSTRAINT CK_Subjects_TotalHours    CHECK (TotalHours IS NULL OR TotalHours >= 0)
);
GO

IF OBJECT_ID(N'dbo.TeacherSubjects', N'U') IS NULL
CREATE TABLE dbo.TeacherSubjects (
    Id         INT IDENTITY(1,1) NOT NULL,
    TeacherId  INT               NOT NULL,
    SubjectId  INT               NOT NULL,
    Section    NVARCHAR(20)      NOT NULL,   -- "comision"

    CONSTRAINT PK_TeacherSubjects           PRIMARY KEY (Id),
    CONSTRAINT UQ_TeacherSubjects           UNIQUE (TeacherId, SubjectId, Section),
    CONSTRAINT FK_TeacherSubjects_Teachers  FOREIGN KEY (TeacherId) REFERENCES dbo.Teachers (Id),
    CONSTRAINT FK_TeacherSubjects_Subjects  FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects (Id),
    INDEX IX_TeacherSubjects_SubjectId (SubjectId)
);
GO

IF OBJECT_ID(N'dbo.StudentSubjects', N'U') IS NULL
CREATE TABLE dbo.StudentSubjects (
    Id         INT IDENTITY(1,1) NOT NULL,
    StudentId  INT               NOT NULL,
    SubjectId  INT               NOT NULL,
    Section    NVARCHAR(20)      NOT NULL,   -- "comision"

    CONSTRAINT PK_StudentSubjects           PRIMARY KEY (Id),
    CONSTRAINT UQ_StudentSubjects           UNIQUE (StudentId, SubjectId),
    CONSTRAINT FK_StudentSubjects_Students  FOREIGN KEY (StudentId) REFERENCES dbo.Students (Id),
    CONSTRAINT FK_StudentSubjects_Subjects  FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects (Id),
    INDEX IX_StudentSubjects_SubjectId (SubjectId)
);
GO

IF OBJECT_ID(N'dbo.Exams', N'U') IS NULL
CREATE TABLE dbo.Exams (
    Id         INT IDENTITY(1,1) NOT NULL,
    SubjectId  INT               NOT NULL,
    TeacherId  INT               NOT NULL,
    ExamDate   DATETIME2(0)      NOT NULL,
    ExamType   NVARCHAR(30)      NOT NULL,

    CONSTRAINT PK_Exams           PRIMARY KEY (Id),
    CONSTRAINT FK_Exams_Subjects  FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects (Id),
    CONSTRAINT FK_Exams_Teachers  FOREIGN KEY (TeacherId) REFERENCES dbo.Teachers (Id),
    INDEX IX_Exams_SubjectId (SubjectId),
    INDEX IX_Exams_TeacherId (TeacherId)
);
GO

/* =============================================================================
   4. Subject programs (syllabus) and their units
   ============================================================================= */

IF OBJECT_ID(N'dbo.SubjectPrograms', N'U') IS NULL
CREATE TABLE dbo.SubjectPrograms (
    Id                       INT IDENTITY(1,1) NOT NULL,
    SubjectId                INT               NOT NULL,
    TeacherId                INT               NOT NULL,
    AcademicYear             INT               NOT NULL,   -- "ciclo lectivo"
    SpecificObjectives       NVARCHAR(MAX)     NULL,
    GeneralObjectives        NVARCHAR(MAX)     NULL,
    WeeklyHours              NVARCHAR(50)      NULL,
    TermHours                NVARCHAR(50)      NULL,
    Assessment               NVARCHAR(MAX)     NULL,
    AssessmentCriteria       NVARCHAR(MAX)     NULL,
    TeachingStrategies       NVARCHAR(MAX)     NULL,
    RemoteSupportStrategies  NVARCHAR(MAX)     NULL,
    RegularConditions        NVARCHAR(MAX)     NULL,
    PromotionConditions      NVARCHAR(MAX)     NULL,
    FreeStudentConditions    NVARCHAR(MAX)     NULL,
    VirtualExams             NVARCHAR(MAX)     NULL,
    CurricularFormat         NVARCHAR(50)      NULL,
    SubjectCondition         NVARCHAR(50)      NULL,

    CONSTRAINT PK_SubjectPrograms               PRIMARY KEY (Id),
    CONSTRAINT UQ_SubjectPrograms               UNIQUE (SubjectId, TeacherId, AcademicYear),
    CONSTRAINT CK_SubjectPrograms_AcademicYear  CHECK (AcademicYear BETWEEN 2000 AND 2100),
    CONSTRAINT FK_SubjectPrograms_Subjects      FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects (Id),
    CONSTRAINT FK_SubjectPrograms_Teachers      FOREIGN KEY (TeacherId) REFERENCES dbo.Teachers (Id),
    INDEX IX_SubjectPrograms_TeacherId (TeacherId)
);
GO

IF OBJECT_ID(N'dbo.ProgramContents', N'U') IS NULL
CREATE TABLE dbo.ProgramContents (
    Id                         INT IDENTITY(1,1) NOT NULL,
    ProgramId                  INT               NOT NULL,
    UnitNumber                 INT               NOT NULL,
    UnitTitle                  NVARCHAR(200)     NOT NULL,
    Content                    NVARCHAR(MAX)     NULL,
    MandatoryBibliography      NVARCHAR(MAX)     NULL,
    SupplementaryBibliography  NVARCHAR(MAX)     NULL,

    CONSTRAINT PK_ProgramContents                  PRIMARY KEY (Id),
    CONSTRAINT UQ_ProgramContents                  UNIQUE (ProgramId, UnitNumber),
    CONSTRAINT CK_ProgramContents_UnitNumber       CHECK (UnitNumber > 0),
    CONSTRAINT FK_ProgramContents_SubjectPrograms  FOREIGN KEY (ProgramId) REFERENCES dbo.SubjectPrograms (Id)
);
GO

/* =============================================================================
   5. Documents and absence justifications
   ============================================================================= */

IF OBJECT_ID(N'dbo.DocumentTypes', N'U') IS NULL
CREATE TABLE dbo.DocumentTypes (
    Id    INT IDENTITY(1,1) NOT NULL,
    Name  NVARCHAR(100)     NOT NULL,

    CONSTRAINT PK_DocumentTypes       PRIMARY KEY (Id),
    CONSTRAINT UQ_DocumentTypes_Name  UNIQUE (Name)
);
GO

IF OBJECT_ID(N'dbo.RoleDocumentTypes', N'U') IS NULL
CREATE TABLE dbo.RoleDocumentTypes (
    Id              INT IDENTITY(1,1) NOT NULL,
    RoleId          INT               NOT NULL,
    DocumentTypeId  INT               NOT NULL,
    IsMandatory     BIT               NOT NULL CONSTRAINT DF_RoleDocumentTypes_IsMandatory DEFAULT (0),
    IsAnnual        BIT               NOT NULL CONSTRAINT DF_RoleDocumentTypes_IsAnnual DEFAULT (0),

    CONSTRAINT PK_RoleDocumentTypes                PRIMARY KEY (Id),
    CONSTRAINT UQ_RoleDocumentTypes                UNIQUE (RoleId, DocumentTypeId),
    CONSTRAINT FK_RoleDocumentTypes_Roles          FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id),
    CONSTRAINT FK_RoleDocumentTypes_DocumentTypes  FOREIGN KEY (DocumentTypeId) REFERENCES dbo.DocumentTypes (Id),
    INDEX IX_RoleDocumentTypes_DocumentTypeId (DocumentTypeId)
);
GO

IF OBJECT_ID(N'dbo.UserDocuments', N'U') IS NULL
CREATE TABLE dbo.UserDocuments (
    Id                   INT IDENTITY(1,1) NOT NULL,
    UserId               INT               NOT NULL,
    DocumentTypeId       INT               NOT NULL,
    FilePath             NVARCHAR(500)     NOT NULL,
    UploadedAt           DATETIME2(0)      NOT NULL CONSTRAINT DF_UserDocuments_UploadedAt DEFAULT (SYSDATETIME()),
    ExpiresAt            DATETIME2(0)      NULL,
    Status               NVARCHAR(20)      NOT NULL,
    SubmittedPhysically  BIT               NOT NULL CONSTRAINT DF_UserDocuments_SubmittedPhysically DEFAULT (0),
    Comment              NVARCHAR(500)     NULL,

    CONSTRAINT PK_UserDocuments                PRIMARY KEY (Id),
    CONSTRAINT CK_UserDocuments_ExpiresAt      CHECK (ExpiresAt IS NULL OR ExpiresAt >= UploadedAt),
    CONSTRAINT FK_UserDocuments_Users          FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_UserDocuments_DocumentTypes  FOREIGN KEY (DocumentTypeId) REFERENCES dbo.DocumentTypes (Id),
    INDEX IX_UserDocuments_UserId (UserId),
    INDEX IX_UserDocuments_DocumentTypeId (DocumentTypeId)
);
GO

IF OBJECT_ID(N'dbo.AbsenceJustifications', N'U') IS NULL
CREATE TABLE dbo.AbsenceJustifications (
    Id              INT IDENTITY(1,1) NOT NULL,
    UserId          INT               NOT NULL,
    AuditorUserId   INT               NULL,   -- who reviewed it
    AbsenceType     NVARCHAR(50)      NOT NULL,
    FilePath        NVARCHAR(500)     NULL,
    AdditionalNote  NVARCHAR(500)     NULL,
    UploadedAt      DATETIME2(0)      NOT NULL CONSTRAINT DF_AbsenceJustifications_UploadedAt DEFAULT (SYSDATETIME()),
    Status          NVARCHAR(20)      NOT NULL,

    CONSTRAINT PK_AbsenceJustifications               PRIMARY KEY (Id),
    CONSTRAINT FK_AbsenceJustifications_Users         FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_AbsenceJustifications_AuditorUsers  FOREIGN KEY (AuditorUserId) REFERENCES dbo.Users (Id),
    INDEX IX_AbsenceJustifications_UserId (UserId),
    INDEX IX_AbsenceJustifications_AuditorUserId (AuditorUserId)
);
GO

/* =============================================================================
   6. Study plans and prerequisites
   ============================================================================= */

IF OBJECT_ID(N'dbo.StudyPlans', N'U') IS NULL
CREATE TABLE dbo.StudyPlans (
    Id                   INT IDENTITY(1,1) NOT NULL,
    Name                 NVARCHAR(150)     NOT NULL,
    PlanCode             NVARCHAR(30)      NOT NULL,
    CareerDurationYears  INT               NULL,
    AttendanceMode       NVARCHAR(50)      NULL,   -- "modalidad de cursada"
    TotalWorkloadHours   INT               NULL,   -- "carga horaria"
    StartYear            INT               NOT NULL,
    EndYear              INT               NULL,
    IsActive             BIT               NOT NULL CONSTRAINT DF_StudyPlans_IsActive DEFAULT (1),

    CONSTRAINT PK_StudyPlans                      PRIMARY KEY (Id),
    CONSTRAINT UQ_StudyPlans_PlanCode             UNIQUE (PlanCode),
    CONSTRAINT CK_StudyPlans_CareerDurationYears  CHECK (CareerDurationYears IS NULL OR CareerDurationYears > 0),
    CONSTRAINT CK_StudyPlans_TotalWorkloadHours   CHECK (TotalWorkloadHours IS NULL OR TotalWorkloadHours >= 0),
    CONSTRAINT CK_StudyPlans_Years                CHECK (EndYear IS NULL OR EndYear >= StartYear)
);
GO

IF OBJECT_ID(N'dbo.StudyPlanSubjects', N'U') IS NULL
CREATE TABLE dbo.StudyPlanSubjects (
    Id           INT IDENTITY(1,1) NOT NULL,
    StudyPlanId  INT               NOT NULL,
    SubjectId    INT               NOT NULL,
    CourseYear   INT               NOT NULL,   -- 1st, 2nd, 3rd year...
    Term         TINYINT           NULL,       -- 1 or 2; NULL for annual subjects

    CONSTRAINT PK_StudyPlanSubjects             PRIMARY KEY (Id),
    CONSTRAINT UQ_StudyPlanSubjects             UNIQUE (StudyPlanId, SubjectId),
    CONSTRAINT CK_StudyPlanSubjects_CourseYear  CHECK (CourseYear BETWEEN 1 AND 10),
    CONSTRAINT CK_StudyPlanSubjects_Term        CHECK (Term IS NULL OR Term IN (1, 2)),
    CONSTRAINT FK_StudyPlanSubjects_StudyPlans  FOREIGN KEY (StudyPlanId) REFERENCES dbo.StudyPlans (Id),
    CONSTRAINT FK_StudyPlanSubjects_Subjects    FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects (Id),
    INDEX IX_StudyPlanSubjects_SubjectId (SubjectId)
);
GO

IF OBJECT_ID(N'dbo.Prerequisites', N'U') IS NULL
CREATE TABLE dbo.Prerequisites (
    Id                     INT IDENTITY(1,1) NOT NULL,
    StudyPlanId            INT               NOT NULL,
    SubjectId              INT               NOT NULL,   -- the subject that has the requirement
    PrerequisiteSubjectId  INT               NOT NULL,   -- the subject that must be passed first

    CONSTRAINT PK_Prerequisites                       PRIMARY KEY (Id),
    CONSTRAINT UQ_Prerequisites                       UNIQUE (StudyPlanId, SubjectId, PrerequisiteSubjectId),
    CONSTRAINT CK_Prerequisites_NotSelf               CHECK (SubjectId <> PrerequisiteSubjectId),
    CONSTRAINT FK_Prerequisites_StudyPlans            FOREIGN KEY (StudyPlanId) REFERENCES dbo.StudyPlans (Id),
    CONSTRAINT FK_Prerequisites_Subjects              FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects (Id),
    CONSTRAINT FK_Prerequisites_PrerequisiteSubjects  FOREIGN KEY (PrerequisiteSubjectId) REFERENCES dbo.Subjects (Id),
    INDEX IX_Prerequisites_SubjectId (SubjectId),
    INDEX IX_Prerequisites_PrerequisiteSubjectId (PrerequisiteSubjectId)
);
GO

/* =============================================================================
   7. Assessments, grades and course records
   ============================================================================= */

IF OBJECT_ID(N'dbo.AssessmentInstances', N'U') IS NULL
CREATE TABLE dbo.AssessmentInstances (
    Id                  INT IDENTITY(1,1) NOT NULL,
    StudyPlanSubjectId  INT               NOT NULL,
    InstanceType        NVARCHAR(30)      NOT NULL,   -- midterm, retake...
    Number              INT               NOT NULL,
    RetakeOfInstanceId  INT               NULL,       -- the instance this one retakes
    InstanceDate        DATETIME2(0)      NULL,

    CONSTRAINT PK_AssessmentInstances                    PRIMARY KEY (Id),
    CONSTRAINT CK_AssessmentInstances_Number             CHECK (Number > 0),
    CONSTRAINT CK_AssessmentInstances_NotSelf            CHECK (RetakeOfInstanceId IS NULL OR RetakeOfInstanceId <> Id),
    CONSTRAINT FK_AssessmentInstances_StudyPlanSubjects  FOREIGN KEY (StudyPlanSubjectId) REFERENCES dbo.StudyPlanSubjects (Id),
    CONSTRAINT FK_AssessmentInstances_RetakeOf           FOREIGN KEY (RetakeOfInstanceId) REFERENCES dbo.AssessmentInstances (Id),
    INDEX IX_AssessmentInstances_StudyPlanSubjectId (StudyPlanSubjectId),
    INDEX IX_AssessmentInstances_RetakeOfInstanceId (RetakeOfInstanceId)
);
GO

IF OBJECT_ID(N'dbo.PartialGrades', N'U') IS NULL
CREATE TABLE dbo.PartialGrades (
    Id                    INT IDENTITY(1,1) NOT NULL,
    StudentSubjectId      INT               NOT NULL,
    AssessmentInstanceId  INT               NOT NULL,
    Grade                 DECIMAL(4,2)      NOT NULL,
    RecordedAt            DATETIME2(0)      NOT NULL CONSTRAINT DF_PartialGrades_RecordedAt DEFAULT (SYSDATETIME()),
    RecordedByTeacherId   INT               NOT NULL,

    CONSTRAINT PK_PartialGrades                      PRIMARY KEY (Id),
    CONSTRAINT UQ_PartialGrades                      UNIQUE (StudentSubjectId, AssessmentInstanceId),
    CONSTRAINT CK_PartialGrades_Grade                CHECK (Grade BETWEEN 0 AND 10),
    CONSTRAINT FK_PartialGrades_StudentSubjects      FOREIGN KEY (StudentSubjectId) REFERENCES dbo.StudentSubjects (Id),
    CONSTRAINT FK_PartialGrades_AssessmentInstances  FOREIGN KEY (AssessmentInstanceId) REFERENCES dbo.AssessmentInstances (Id),
    CONSTRAINT FK_PartialGrades_Teachers             FOREIGN KEY (RecordedByTeacherId) REFERENCES dbo.Teachers (Id),
    INDEX IX_PartialGrades_AssessmentInstanceId (AssessmentInstanceId),
    INDEX IX_PartialGrades_RecordedByTeacherId (RecordedByTeacherId)
);
GO

IF OBJECT_ID(N'dbo.CourseRecords', N'U') IS NULL
CREATE TABLE dbo.CourseRecords (
    Id                    INT IDENTITY(1,1) NOT NULL,
    StudentSubjectId      INT               NOT NULL,
    AttendancePercentage  DECIMAL(5,2)      NULL,
    NumericGrade          DECIMAL(4,2)      NULL,
    LetterGrade           NVARCHAR(20)      NULL,
    FinalStatus           NVARCHAR(30)      NULL,   -- "condicion final": regular, promoted, free...
    Observations          NVARCHAR(1000)    NULL,
    UpdatedAt             DATETIME2(0)      NOT NULL CONSTRAINT DF_CourseRecords_UpdatedAt DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_CourseRecords                   PRIMARY KEY (Id),
    CONSTRAINT UQ_CourseRecords_StudentSubjectId  UNIQUE (StudentSubjectId),
    CONSTRAINT CK_CourseRecords_Attendance        CHECK (AttendancePercentage IS NULL OR AttendancePercentage BETWEEN 0 AND 100),
    CONSTRAINT CK_CourseRecords_NumericGrade      CHECK (NumericGrade IS NULL OR NumericGrade BETWEEN 0 AND 10),
    CONSTRAINT FK_CourseRecords_StudentSubjects   FOREIGN KEY (StudentSubjectId) REFERENCES dbo.StudentSubjects (Id)
);
GO

/* =============================================================================
   8. Initial data: one role per profile in the UML
   ============================================================================= */

INSERT INTO dbo.Roles (Name)
SELECT v.Name
FROM (VALUES (N'Director'), (N'AcademicSecretary'), (N'Teacher'), (N'Student')) AS v (Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles AS r WHERE r.Name = v.Name);
GO
