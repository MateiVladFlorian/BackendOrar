namespace BackendOrar.Models.Filters
{
    public class TimetableFilterModel
    {
        public int? year { get; set; } /* study year */
        public string? cycle { get; set; } /* study cycle */
        public string? groupCode { get; set; } /* group code */
        public string? pname { get; set; } /* professor name */
        public string? cname { get; set; } /* course name */
        public string? rname { get; set; } /* room name */
    }
}
