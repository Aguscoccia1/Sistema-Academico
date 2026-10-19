namespace AcademicSystem.Entities.DTOs;

public class StudentObjectResponseDto
{
    private int id;
    private int userId;
    private int roleId;
    private string firstName = "";
    private string lastName = "";
    private string cuil = "";
    private string dni = "";
    private string recordNumber = "";
    private string email = "";
    private DateOnly? birthDate;
    private string? address;
    private string? phone;
    private string? emergencyContactPhone;
    private bool isActive;


    

    public int Id { get { return id; } set { id = value; } }
    public int UserId { get { return userId; } set { userId = value; } }
    public int RoleId { get { return roleId; } set { roleId = value; } }
    public string FirstName { get { return firstName; } set { firstName = value; } }
    public string LastName { get { return lastName; } set { lastName = value; } }
    public string Cuil { get { return cuil; } set { cuil = value; } }
    public string Dni { get { return dni; } set { dni = value; } }
    public string RecordNumber { get { return recordNumber; } set { recordNumber = value; } }
    public string Email { get { return email; } set { email = value; } }
    public DateOnly? BirthDate { get { return birthDate; } set { birthDate = value; } }
    public string? Address { get { return address; } set { address = value; } }
    public string? Phone { get { return phone; } set { phone = value; } }
    public string? EmergencyContactPhone { get { return emergencyContactPhone; } set { emergencyContactPhone = value; } }
    public bool IsActive { get { return isActive; } set { isActive = value; } }
}
