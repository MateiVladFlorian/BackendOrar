using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable

namespace BackendOrar.Data
{
    public class Professor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        [ForeignKey("ProfessorId")]
        public virtual ICollection<Timetable> Timetables { get; set; }
    }
}
