namespace Shibusa.DevTools.CsProjects.Cli
{
    internal class CodeReference : IComparable<CodeReference>, IEquatable<CodeReference?>
    {
        public CodeReference(string name, CodeReferenceType codeReferenceType, string? version = null, int ordinalPosition = 0)
        {
            Name = name;
            CodeReferenceType = codeReferenceType;
            Version = new Version(version);
            OrdinalPosition = ordinalPosition;
            Children = new List<CodeReference>();
        }

        public string Name { get; }
        public Version Version { get; }
        public IList<CodeReference> Children { get; }
        public CodeReferenceType CodeReferenceType { get; }
       
        public int OrdinalPosition { get; set; } = 0;

        public int CompareTo(CodeReference? other)
        {
            if (other == null) return 1;

            var result = OrdinalPosition.CompareTo(other.OrdinalPosition);
            if (result == 0) result = CodeReferenceType.CompareTo(other.CodeReferenceType);
            if (result == 0) result = Name.CompareTo(other.Name);
            if (result == 0) result = Version.Compare(Version, other.Version);
            if (result == 0) result = Children.Count.CompareTo(other.Children.Count);

            return result;
        }

        public bool ContainsInChain(string name)
        {
            if (Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) { return true; }

            foreach (var child in Children)
            {
                if (child.ContainsInChain(name)) return true;
            }

            return false;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CodeReference);
        }

        public bool Equals(CodeReference? other)
        {
            return other is not null &&
                   Name == other.Name &&
                   Version.Equals(other.Version) &&
                   CodeReferenceType == other.CodeReferenceType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Version, CodeReferenceType);
        }

        public override string ToString()
        {
            return $"{CodeReferenceType}: {Name} {Version}".Trim();
        }
    }

    internal enum CodeReferenceType
    {
        None = 0,
        Project = 1,
        NuGet = 2
    }
}
