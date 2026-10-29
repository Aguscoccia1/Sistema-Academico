//(damian)
namespace AcademicSystem.Entities.DTOs
{
    public class LoginResponseDto
    {
        private int id;
        private string fullName = "";
        private string email = "";
        private string role = "";
        private string recordNumber = "";
        private string token = "";
        private string refreshToken = "";
        private int expiresIn;

        public int Id { get {return id; } set { id = value; } }
        public string FullName { get {return fullName; } set { fullName = value; } }
        public string Email { get {return email; } set { email = value; } }
        public string Role { get {return role; } set { role = value; } }
        public string RecordNumber { get {return recordNumber; } set { recordNumber = value; } }
        public string Token { get {return token; } set { token = value; } }
        public string RefreshToken { get {return refreshToken; } set { refreshToken = value; } }
        public int ExpiresIn { get {return expiresIn; } set { expiresIn = value; } }
    }
}