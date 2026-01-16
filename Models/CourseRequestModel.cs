using BackendOrar.Models.Filters;
#pragma warning disable

namespace BackendOrar.Models
{
    public class CourseRequestModel
    {
        public string name { get; set; }
        public string type { get; set; }
        public string department { get; set; }
        public string studyCycle { get; set; }
    }
}
