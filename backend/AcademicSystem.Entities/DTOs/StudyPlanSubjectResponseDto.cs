namespace AcademicSystem.Entities.DTOs
{
    public class StudyPlanSubjectResponseDto
    {
        private int id;
        private int studyPlanId;
        private int subjectId;
        private int courseYear;
        private byte? term;
        private SubjectResponseDto subject = new SubjectResponseDto();

        public int Id { get { return id; } set { id = value; } }
        public int StudyPlanId { get { return studyPlanId; } set { studyPlanId = value; } }
        public int SubjectId { get { return subjectId; } set { subjectId = value; } }
        public int CourseYear { get { return courseYear; } set { courseYear = value; } }
        public byte? Term { get { return term; } set { term = value; } }
        public SubjectResponseDto Subject { get { return subject; } set { subject = value; } }
    }
}