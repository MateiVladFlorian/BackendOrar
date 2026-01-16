#pragma warning disable

namespace BackendOrar.Models
{
    public class MetadataModel
    {
        public string academicYear { get; set; } // academic year
        public MetaGroupModel[] cycles { get; set; }
    }

    public class MetaGroupModel
    { 
        public string studyCycle { get; set; } // study cycle
        public MetaYearModel[] years { get; set; }
    }

    public class MetaYearModel
    {
        public string studyYear { get; set; } // study year
        public string[] groups { get; set; } // groupCode list
    }
}
