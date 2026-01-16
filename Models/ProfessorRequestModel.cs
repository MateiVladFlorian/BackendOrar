#pragma warning disable

using BackendOrar.Models.Filters;

namespace BackendOrar.Models
{
    public class ProfessorRequestModel
    {
        public string name { get; set; }
        public string email { get; set; }
        public string position { get; set; }
        public string department { get; set; }
    }
}
