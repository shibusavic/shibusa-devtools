using System.Text.RegularExpressions;

namespace Shibusa.DevTools.CsProjects.Cli
{
    internal class CodeProjectFile : IEquatable<CodeProjectFile?>
    {
        private const string PackageIdRegEx = @"<PackageId>\s*?([^""]+)\s*?<\/PackageId>";
        private const string ProjectRefRegEx = @"<ProjectReference\s*?Include=\""([^""]+)\""\s?\/>";
        private const string PackageRefIncludeRegEx = @"<PackageReference.+?Include=\""([^""]+)\"".+?\/>";
        private const string PackageRefVersionRegEx = @"Version=\""([^""]+)\""";

        public CodeProjectFile(FileInfo fileInfo)
        {
            FileInfo = fileInfo;

            if (FileInfo.Exists)
            {
                var text = File.ReadAllText(FileInfo.FullName);

                var regex = new Regex(PackageIdRegEx, RegexOptions.Multiline);

                if (regex.IsMatch(text))
                {
                    PackageId = regex.Match(text).Groups[1].Value;
                }

                regex = new Regex(ProjectRefRegEx, RegexOptions.Multiline);

                var matches = regex.Matches(text);

                if (matches.Any())
                {
                    List<string> projectRefs = new();

                    foreach (Match match in matches)
                    {
                        projectRefs.Add(match.Groups[1].Value);
                    }

                    ProjectReferences = projectRefs.ToArray();
                }

                regex = new Regex(PackageRefIncludeRegEx, RegexOptions.Multiline);
                matches = regex.Matches(text);

                if (matches.Any())
                {
                    List<(string Ref, string? Version)> packageRefs = new();

                    foreach (Match match in matches)
                    {
                        var refText = match.Groups[1].Value;
                        string? versionText = null;
                        var versionRegex = new Regex(PackageRefVersionRegEx, RegexOptions.Multiline);
                        if (versionRegex.IsMatch(match.Groups[0].Value))
                        {
                            versionText = versionRegex.Match(match.Groups[0].Value).Groups[1].Value;
                        }
                        packageRefs.Add((refText, versionText));
                    }

                    PackageReferences = packageRefs.ToArray();
                }
            }
        }

        public FileInfo FileInfo { get; }

        public string Name => FileInfo.Name;

        public string FullName => FileInfo.FullName;

        public string[] ProjectReferences { get; } = Array.Empty<string>();

        public (string Ref, string? Version)[] PackageReferences { get; } = Array.Empty<(string Ref, string? Version)>();

        public string? PackageId { get; }

        public bool IsNuget => !string.IsNullOrWhiteSpace(PackageId);

        public override bool Equals(object? obj)
        {
            return Equals(obj as CodeProjectFile);
        }

        public bool Equals(CodeProjectFile? other)
        {
            return other is not null &&
                   Name == other.Name &&
                   FullName == other.FullName &&
                   PackageId == other.PackageId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, FullName, PackageId);
        }

        public override string ToString() => IsNuget ? $"nuget: {FileInfo.Name}" : FileInfo.Name;
    }

    internal class ProjectCollection
    {
        private IDictionary<CodeProjectFile, IList<CodeProjectFile>> profileFileDictionary;
        public ProjectCollection()
        {
            Files = new();
            profileFileDictionary = new Dictionary<CodeProjectFile, IList<CodeProjectFile>>();
        }

        public HashSet<CodeProjectFile> Files { get; }

        public void WriteCollectionToStream(Stream stream)
        {
            if (!stream.CanWrite) throw new ArgumentException("Cannot write to provided stream.");
            
            FinalizeDictionary();


        }

        private void FinalizeDictionary()
        {

        }
    }
    //internal record CodeProject
    //{
    //    public CodeProject(FileInfo fileInfo, bool isNuget = false)
    //    {
    //        FileInfo = fileInfo;
    //        IsNuget = isNuget;
    //        Dependents = new List<CodeProject>();
    //    }

    //    public FileInfo FileInfo { get; }
    //    public bool IsNuget { get; }
    //    public H<CodeProject> Dependents { get; }
    //}
}
