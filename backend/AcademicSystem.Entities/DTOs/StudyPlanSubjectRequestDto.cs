namespace AcademicSystem.Entities.DTOs
{
    public class StudyPlanSubjectRequestDto
    {
        private int subjectId;
        private int courseYear;
        private byte? term;

        public int SubjectId { get { return subjectId; } set { subjectId = value; } }
        public int CourseYear { get { return courseYear; } set { courseYear = value; } }
        public byte? Term { get { return term; } set { term = value; } }
    }
}