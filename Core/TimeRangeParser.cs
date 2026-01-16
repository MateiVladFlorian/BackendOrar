using System.Globalization;
#pragma warning disable

namespace BackendOrar.Core
{
    public static class TimeRangeParser
    {
        private static readonly string[] separators = 
            { "-", " - ", "–", " – ", " — ", "—" };

        public static TimeRange? Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            try
            {
                /* try to parse with common separators */
                string[] parts = null;

                foreach (var separator in separators)
                {
                    parts = input.Split(new[] { separator }, 
                        StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                        break;
                }

                if (parts == null || parts.Length != 2)
                    return null;

                var startStr = parts[0].Trim();
                var endStr = parts[1].Trim();

                /* parse times */
                var startTime = ParseTime(startStr);
                var endTime = ParseTime(endStr);

                if (!startTime.HasValue || !endTime.HasValue)
                    return null;

                /* validate that start is before end */
                if (startTime.Value > endTime.Value)
                    return null;

                return new TimeRange(startTime.Value, endTime.Value);
            }
            catch
            {
                return null;
            }
        }

        private static TimeSpan? ParseTime(string timeString)
        {
            /* try parsing as 24-hour format first */
            if (TimeSpan.TryParseExact(timeString, "hh\\:mm", CultureInfo.InvariantCulture, out var time24))
                return time24;

            if (TimeSpan.TryParseExact(timeString, "h\\:mm", CultureInfo.InvariantCulture, out time24))
                return time24;

            /* try parsing as 12-hour format with AM/PM */
            DateTime dateTime;

            string[] formats = {
            "h:mm tt", "h:mmtt", "hh:mm tt", "hh:mmtt",
            "h tt", "htt", "hh tt", "hhtt"
            };

            if (DateTime.TryParseExact(timeString, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.NoCurrentDateDefault, out dateTime))
                return dateTime.TimeOfDay;

            return null;
        }
    }
}
