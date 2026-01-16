using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable

namespace BackendOrar.Data
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Department { get; set; }
        public string StudyCycle { get; set; }
        [ForeignKey("CourseId")]
        public virtual ICollection<Timetable> Timetables { get; set; }
    }
}
