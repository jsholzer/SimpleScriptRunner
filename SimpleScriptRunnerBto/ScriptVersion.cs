using System;

namespace SimpleScriptRunnerBto
{
    [Serializable]
    public class ScriptVersion : IComparable<ScriptVersion>
    {
        public ScriptVersion(int major, int minor, long patch, DateTime date, string machineName, string description)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Date = date;
            MachineName = machineName;
            Description = description;
        }

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public long Patch { get; private set; }
        public DateTime Date { get; set; }
        public String MachineName { get; private set; }
        public String Description { get; private set; }

        public bool IsPatchNumeric { get { return Patch < 1000; } }        // Anything greater than 1000 is assumed to be a timestamp

        public int CompareTo(ScriptVersion other)
        {
            int compare = Major.CompareTo(other.Major);
            if (compare != 0)
                return compare;

            compare = Minor.CompareTo(other.Minor);
            if (compare != 0)
                return compare;

            compare = Patch.CompareTo(other.Patch);
            if (compare != 0)
                return compare;

            return RoundDateTime(Date).CompareTo(RoundDateTime(other.Date));
        }

        public int compareIgnoreDate(ScriptVersion other)
        {
            int compare = Major.CompareTo(other.Major);
            if (compare != 0)
                return compare;

            compare = Minor.CompareTo(other.Minor);
            if (compare != 0)
                return compare;

            return Patch.CompareTo(other.Patch);
        }

        protected bool Equals(ScriptVersion other)
        {
            return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ScriptVersion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Major;
                hashCode = (hashCode * 397) ^ Minor;
                hashCode = (hashCode * 397) ^ Patch.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}, {2}, {3}", Major, Minor, Patch, Date);
        }

        private DateTime RoundDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }
    }
}