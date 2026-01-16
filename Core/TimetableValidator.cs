using BackendOrar.Definitions;
using BackendOrar.Models;
#pragma warning disable

namespace BackendOrar.Core
{
    public class TimetableValidator
    {
        public static bool IsValidTimetableEntry(TimetableRequestModel? entry)
        {
            if (entry == null) return false;
            TimeRange? result = TimeRangeParser.Parse(entry.range);

            return (!string.IsNullOrEmpty(entry.professorName)
                && (!string.IsNullOrEmpty(entry.dayOfWeek) && isDayValid(entry.dayOfWeek))
                && (!string.IsNullOrEmpty(entry.range) && result != null)
                && !string.IsNullOrEmpty(entry.academicYear)
                && !string.IsNullOrEmpty(entry.courseName)
                && !string.IsNullOrEmpty(entry.courseType)
                && !string.IsNullOrEmpty(entry.groupCode)
                && !string.IsNullOrEmpty(entry.studyCycle)
                && !string.IsNullOrEmpty(entry.roomName)
                && (entry.studyYear != null && (entry.studyYear.Value >= (int)TimetableConstants.MinStydyYear // fara !
                && entry.studyYear.Value <= (int)TimetableConstants.MaxStudyYear))
                && !string.IsNullOrEmpty(entry.department)
                && !string.IsNullOrEmpty(entry.buildingName)
                && !string.IsNullOrEmpty(entry.professorEmail)
                && !string.IsNullOrEmpty(entry.professorPosition)
                && entry.classroomCapacity != null);
        }

        public static bool IsValidUpdateBody(TimetableUpdateModel? model)
        {
            if (model == null) return false;
            TimeRange? result = TimeRangeParser.Parse(model.range);

            return (
                    model.course_id != null && model.group_id != null
                    && model.classroom_id != null && model.professor_id != null
                    && !string.IsNullOrEmpty(model.dayOfWeek) && isDayValid(model.dayOfWeek)
                    && !string.IsNullOrEmpty(model.academicYear)
                    && (!string.IsNullOrEmpty(model.range) && result != null));
        }

        private static bool isDayValid(string dayName)
        {
            if (string.IsNullOrEmpty(dayName)) return false;
            var today = dayName.ToLower();

            for (int day = (int)Definitions.DayOfWeek.Monday; day < (int)Definitions.DayOfWeek.Sunday; day++)
            {
                string tempDay = ((Definitions.DayOfWeek)day).ToString().ToLower();
                if (tempDay.CompareTo(today) == 0) return true;
            }

            return false;
        }

        public static Definitions.DayOfWeek? GetDayOfWeek(string dayName)
        {
            if (!string.IsNullOrWhiteSpace(dayName)) return null;
            var today = dayName.ToLower();

            for (int day = (int)Definitions.DayOfWeek.Monday; day < (int)Definitions.DayOfWeek.Sunday; day++)
            {
                string tempDay = ((Definitions.DayOfWeek)day).
                    ToString().ToLower();

                if (tempDay.CompareTo(today) == 0) 
                    return (Definitions.DayOfWeek)day;
            }

            return null;
        }
    }
}
