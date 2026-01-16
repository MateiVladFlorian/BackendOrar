#pragma warning disable
namespace BackendOrar.Models
{
    public class TimetableEntry
    {
        public int id { get; set; }
        public string courseName { get; set; }
        public string studyCycle { get; set; }
        public string roomName { get; set; }
        public string professorName { get; set; }
        public string professorEmail { get; set; }
        public string professorPosition { get; set; }
        public string dayOfWeek { get; set; }
        public string buildingName { get; set; }
        public int classroomCapacity { get; set; }
        public string range { get; set; }
        public string academicYear { get; set; }
        public string courseType { get; set; }
        public string groupCode { get; set; }
        public int studyYear { get; set; }
        public string department { get; set; }
    }
}
