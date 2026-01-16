using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BackendOrar.Core
{
    public class TimeRange
    {
        public TimeSpan Start { get; }
        public TimeSpan End { get; }

        public TimeRange(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }


        public override string ToString()
        {
            return $"{Start:hh\\:mm} - {End:hh\\:mm}";
        }

        public bool AreIntervalsDisjoint(TimeRange? other)
        {
            if (other == null) return false;
            TimeRange self = this;

            return self.End <= other.Start 
                || other.End <= self.Start;
        }
    }
}
