namespace AcademicSystem.Business.Services;

public enum ProfileResultStatus
{
    Success,
    InvalidRequest,
    NotFound,
    Conflict
}

public sealed record ProfileResult<T>(ProfileResultStatus Status, T? Data = default, string? Message = null);
