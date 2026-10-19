using System.Text;
using AcademicSystem.Api;
using AcademicSystem.Api.Security;
using AcademicSystem.Business.Services;
using AcademicSystem.Data.Context;
using AcademicSystem.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Data: the connection string lives in appsettings.json under "ConnectionStrings:AcademicSystem".
var connectionString = builder.Configuration.GetConnectionString("AcademicSystem")
    ?? throw new InvalidOperationException("Missing connection string 'AcademicSystem' in appsettings.json.");

builder.Services.AddDbContext<AcademicSystemContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>(); 

// Business: each team registers its services here, one line per service.
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>(); 
builder.Services.AddScoped<IPermissionService, PermissionService>();
// validacion del JWT. La clave vive en user-secrets, nunca en el repo.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta 'Jwt:Key'. Configurala con user-secrets.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // sin margen: un token vencido da 401 en el momento
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped<IStudyPlanService, StudyPlanService>();

builder.Services.AddScoped<ISubjectService, SubjectService>();

// E4: inscripciones y validacion de correlatividades.
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IPrerequisiteValidationService, PrerequisiteValidationService>();

// Lets the React frontend (Vite, http://localhost:5173) call this API.
const string FrontendCorsPolicy = "Frontend";
var frontendOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(frontendOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    try
    {
        DevelopmentSeeder.SeedLoginUsers(app.Services);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo ejecutar el seeder de desarrollo; la API seguirá iniciando.");
    }
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

app.Run();
