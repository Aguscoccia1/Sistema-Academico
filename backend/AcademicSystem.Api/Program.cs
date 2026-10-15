using AcademicSystem;

using AcademicSystem.Business.Services;
using AcademicSystem.Data.Context;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Data: the connection string lives in appsettings.json under "ConnectionStrings:AcademicSystem".
var connectionString = builder.Configuration.GetConnectionString("AcademicSystem")
    ?? throw new InvalidOperationException("Missing connection string 'AcademicSystem' in appsettings.json.");

builder.Services.AddDbContext<AcademicSystemContext>(options => options.UseSqlServer(connectionString));

// Business: each team registers its services here, one line per service.
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IComisionesService, ComisionesService>();
builder.Services.AddScoped<IInscripcionesService, InscripcionesService>();
builder.Services.AddScoped<IStudentService, StudentService>();

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
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
