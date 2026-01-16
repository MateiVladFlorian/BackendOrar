using BackendOrar.Core;
#pragma warning disable

namespace BackendOrar.Models
{
    public class TimetableUpdateModel
    {
        public int? course_id { get; set; }
        public int? group_id { get; set; }
        public int? professor_id { get; set; }
        public int? classroom_id { get; set; }
        public string? dayOfWeek { get; set; }
        public string? range { get; set; }
        public string? academicYear { get; set; }
    }
}
