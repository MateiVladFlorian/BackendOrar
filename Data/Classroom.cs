using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable

namespace BackendOrar.Data
{
    public class Classroom
    {
        public int Id { get; set; }
        public string RoomName { get; set; }
        public int Capacity { get; set; }
        public string Building { get; set; }
        [ForeignKey("ClassroomId")]
        public virtual ICollection<Timetable> Timetables { get; set; }
    }
}
