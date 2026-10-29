namespace AcademicSystem.Entities.DTOs
{
    public class RefreshRequestDto
    {
        private string refreshToken = "";

        public string RefreshToken { get { return refreshToken; } set { refreshToken = value; } }
    }

    public class RefreshResponseDto
    {
        private string token = "";
        private string refreshToken = "";
        private int expiresIn;

        public string Token { get { return token; } set { token = value; } }
        public string RefreshToken { get { return refreshToken; } set { refreshToken = value; } }
        public int ExpiresIn { get { return expiresIn; } set { expiresIn = value; } }
    }
}
