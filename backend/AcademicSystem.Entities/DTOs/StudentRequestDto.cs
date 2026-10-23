using System.ComponentModel.DataAnnotations;

namespace AcademicSystem.Entities.DTOs;

public class StudentRequestDto
{
    private int userId;
    private string firstName = "";
    private string lastName = "";
    private string dni = "";
    private string recordNumber = "";


    

    [Range(1, int.MaxValue)]
    public int UserId { get { return userId; } set { userId = value; } }

    [Required, StringLength(100)]
    public string FirstName { get { return firstName; } set { firstName = value; } }

    [Required, StringLength(100)]
    public string LastName { get { return lastName; } set { lastName = value; } }

    [Required, RegularExpression(@"^\d{7,8}$")]
    public string Dni { get { return dni; } set { dni = value; } }

    [Required, StringLength(20)]
    public string RecordNumber { get { return recordNumber; } set { recordNumber = value; } }
}
