namespace AcademicSystem.Entities.DTOs
{
    public class SubjectRequestDto
    {
        private string name = "";
        private int? orderNumber;
        private string? format;
        private int? lectureHours;
        private int? totalHours;
        private string? course;

        public string Name { get { return name; } set { name = value; } }
        public int? OrderNumber { get { return orderNumber; } set { orderNumber = value; } }
        public string? Format { get { return format; } set { format = value; } }
        public int? LectureHours { get { return lectureHours; } set { lectureHours = value; } }
        public int? TotalHours { get { return totalHours; } set { totalHours = value; } }
        public string? Course { get { return course; } set { course = value; } }
    }
}