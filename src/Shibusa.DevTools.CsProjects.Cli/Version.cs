namespace Shibusa.DevTools.CsProjects.Cli
{
    internal struct Version : IComparer<Version>, IEquatable<Version>
    {
        public Version(string? versionText)
        {
            if (!string.IsNullOrWhiteSpace(versionText))
            {

                try
                {
                    string[] split = versionText.Split('.');

                    foreach (var item in split)
                    {
                        if (!Major.HasValue)
                        {
                            Major = Convert.ToInt32(item.Trim());
                            continue;
                        }
                        if (!Minor.HasValue)
                        {
                            Minor = Convert.ToInt32(item.Trim());
                            continue;
                        }
                        if (!Build.HasValue)
                        {
                            Build = Convert.ToInt32(item.Trim());
                            break;
                        }
                    }

                    MoveValuesLeft(ref Major, ref Minor, ref Build);
                }
                catch
                {
                    throw new ArgumentException("Version seems malformed; should be integers separated by dots.");
                }
            }
        }

        
        public Version(int? major = null, int? minor = null, int? build = null)
        {
            Major = major;
            Minor = minor;
            Build = build;

            MoveValuesLeft(ref Major, ref Minor, ref Build);
        }

        public static Version Empty => new();

        public bool IsEmpty => Major == Minor && Minor == Build && Build == null;

        public static Version Zeros => new Version(0, 0, 0);

        public bool IsZero => Major.HasValue && Major.Value == 0 &&
            Minor.HasValue && Minor.Value == 0 &&
            Build.HasValue && Build.Value == 0;

        public bool IsZeroOrEmpty => IsZero || IsEmpty;

        public int? Major = null;
        public int? Minor = null;
        public int? Build = null;

        private static void MoveValuesLeft(ref int? major, ref int? minor, ref int? build)
        {
            bool change = false;

            if (minor.HasValue && !major.HasValue)
            {
                major = minor;
                minor = null;
                change = true;
            }

            if (build.HasValue && !minor.HasValue)
            {
                minor = build;
                build = null;
                change = true;
            }

            if (change) MoveValuesLeft(ref major, ref minor, ref build);
        }

        public int Compare(Version x, Version y)
        {
            int result = x.Major.GetValueOrDefault().CompareTo(y.Major.GetValueOrDefault());
            if (result == 0) result = x.Minor.GetValueOrDefault().CompareTo(y.Minor.GetValueOrDefault());
            if (result == 0) result = x.Build.GetValueOrDefault().CompareTo(y.Build.GetValueOrDefault());

            return result;
        }

        public override string ToString()
        {
            if (IsEmpty) return string.Empty;

            List<string> sections = new();

            if (Major.HasValue) sections.Add(Major.Value.ToString());
            if (Minor.HasValue) sections.Add(Minor.Value.ToString());
            if (Build.HasValue) sections.Add(Build.Value.ToString());

            return string.Join('.', sections);
        }

        public override bool Equals(object? obj)
        {
            return obj is Version version && Equals(version);
        }

        public bool Equals(Version other)
        {
            return Major == other.Major &&
                   Minor == other.Minor &&
                   Build == other.Build;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Build);
        }

        public static bool operator ==(Version left, Version right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Version left, Version right)
        {
            return !(left == right);
        }

        public static bool operator >(Version left, Version right)
        {
            return left.Major.GetValueOrDefault() > right.Major.GetValueOrDefault()
                ? true
                : left.Minor.GetValueOrDefault() > right.Minor.GetValueOrDefault()
                ? true
                : left.Build.GetValueOrDefault() > right.Build.GetValueOrDefault()
                ? true
                : false;
        }

        public static bool operator <(Version left, Version right)
        {
            return left.Major.GetValueOrDefault() < right.Major.GetValueOrDefault()
                ? true
                : left.Minor.GetValueOrDefault() < right.Minor.GetValueOrDefault()
                ? true
                : left.Build.GetValueOrDefault() < right.Build.GetValueOrDefault()
                ? true
                : false;
        }

        public static bool operator >=(Version left, Version right)
        {
            return left > right || left == right;
        }

        public static bool operator <=(Version left, Version right)
        {
            return left < right || left == right;
        }
    }
}
