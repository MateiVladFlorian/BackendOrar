using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable

namespace BackendOrar.Data
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public int StudyYear { get; set; }
        public string StudyCycle { get; set; }
        [ForeignKey("GroupId")]
        public virtual ICollection<Timetable> Timetables { get; set; }
    }
}
