namespace AcademicSystem.Entities.DTOs
{
    public class UserRequestDto
    {
        private string firstName = "";
        private string lastName = "";
        private string cuil = "";
        private DateOnly? birthDate;
        private string? address;
        private int roleId;
        private string email = "";
        private string? phone;
        private string? emergencyContactPhone;
        private string? password;
        private bool isActive;

        public string FirstName { get { return firstName; } set { firstName = value; } }
        public string LastName { get { return lastName; } set { lastName = value; } }
        public string Cuil { get { return cuil; } set { cuil = value; } }
        public DateOnly? BirthDate { get { return birthDate; } set { birthDate = value; } }
        public string? Address { get { return address; } set { address = value; } }
        public int RoleId { get { return roleId; } set { roleId = value; } }
        public string Email { get { return email; } set { email = value; } }
        public string? Phone { get { return phone; } set { phone = value; } }
        public string? EmergencyContactPhone { get { return emergencyContactPhone; } set { emergencyContactPhone = value; } }
        public string? Password { get { return password; } set { password = value; } }
        public bool IsActive { get { return isActive; } set { isActive = value; } }
    }
}
