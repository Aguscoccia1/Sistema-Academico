namespace AcademicSystem.Entities.DTOs;
public class StudentResponseDto
{
    private int id;
    private int userId;
    private string firstName = "";
    private string lastName = "";
    private string dni = "";
    private string recordNumber = "";


    

    public int Id { get { return id; } set { id = value; } }
    public int UserId { get { return userId; } set { userId = value; } }
    public string FirstName { get { return firstName; } set { firstName = value; } }
    public string LastName { get { return lastName; } set { lastName = value; } }
    public string Dni { get { return dni; } set { dni = value; } }
    public string RecordNumber { get { return recordNumber; } set { recordNumber = value; } }
}