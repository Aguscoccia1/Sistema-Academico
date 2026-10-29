//(damian)
namespace AcademicSystem.Entities.DTOs
{
    public class LoginRequestDto
    {
        private string email = "";
        private string password = "";
        private bool remember;

        public string Email { get {return email; } set { email = value; } }
        public string Password { get {return password; } set { password = value; } }
        public bool Remember { get {return remember; } set { remember = value; } }
    }
}