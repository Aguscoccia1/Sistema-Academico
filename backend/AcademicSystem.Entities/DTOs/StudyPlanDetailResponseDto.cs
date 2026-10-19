namespace AcademicSystem.Entities.DTOs
{
    public class StudyPlanDetailResponseDto
    {
        private int id;
        private string name = "";
        private string planCode = "";
        private int? careerDurationYears;
        private string? attendanceMode;
        private int? totalWorkloadHours;
        private int startYear;
        private int? endYear;
        private bool isActive;
        private List<StudyPlanSubjectResponseDto> subjects = new List<StudyPlanSubjectResponseDto>();

        public int Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public string PlanCode { get { return planCode; } set { planCode = value; } }
        public int? CareerDurationYears { get { return careerDurationYears; } set { careerDurationYears = value; } }
        public string? AttendanceMode { get { return attendanceMode; } set { attendanceMode = value; } }
        public int? TotalWorkloadHours { get { return totalWorkloadHours; } set { totalWorkloadHours = value; } }
        public int StartYear { get { return startYear; } set { startYear = value; } }
        public int? EndYear { get { return endYear; } set { endYear = value; } }
        public bool IsActive { get { return isActive; } set { isActive = value; } }
        public List<StudyPlanSubjectResponseDto> Subjects { get { return subjects; } set { subjects = value; } }
    }
}