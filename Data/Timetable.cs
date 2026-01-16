#pragma warning disable

namespace BackendOrar.Data
{
    public class Timetable
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int? ClassroomId { get; set; }
        public int ProfessorId { get; set; }
        public int CourseId { get; set; }
        public Group Group { get; set; }
        public Classroom Classroom { get; set; }
        public Professor Professor { get; set; }
        public Course Course { get; set; }
        public string DayOfWeek { get; set; }
        public string Range { get; set; }
        public string AcademicYear { get; set; }
    }
}
