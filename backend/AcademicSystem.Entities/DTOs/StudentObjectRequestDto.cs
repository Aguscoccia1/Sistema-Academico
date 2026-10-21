using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Entities.DTOs;

public class StudentObjectRequestDto
{
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
    private string? password;
    private int roleId;
    private bool isActive = true;



    [Required, StringLength(100)]
    public string FirstName { get { return firstName; } set { firstName = value; } }

    [Required, StringLength(100)]
    public string LastName { get { return lastName; } set { lastName = value; } }

    [Required, RegularExpression(@"^\d{11}$")]
    public string Cuil { get { return cuil; } set { cuil = value; } }

    [Required, RegularExpression(@"^\d{7,8}$")]
    public string Dni { get { return dni; } set { dni = value; } }

    [Required, StringLength(20)]
    public string RecordNumber { get { return recordNumber; } set { recordNumber = value; } }

    [Required, EmailAddress, StringLength(255)]
    public string Email { get { return email; } set { email = value; } }

    public DateOnly? BirthDate { get { return birthDate; } set { birthDate = value; } }

    [StringLength(200)]
    public string? Address { get { return address; } set { address = value; } }

    [StringLength(20)]
    public string? Phone { get { return phone; } set { phone = value; } }

    [StringLength(20)]
    public string? EmergencyContactPhone { get { return emergencyContactPhone; } set { emergencyContactPhone = value; } }

    [MinLength(8)]
    public string? Password { get { return password; } set { password = value; } }

    [Range(1, int.MaxValue)]
    public int RoleId { get { return roleId; } set { roleId = value; } }

    public bool IsActive { get { return isActive; } set { isActive = value; } }
}
