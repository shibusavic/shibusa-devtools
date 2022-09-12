using Shibusa.DevTools.AppServices;
using Shibusa.DevTools.CsProjects.Cli;
using System.Diagnostics;
using System.Reflection;

bool showHelp = false;
int exitCode = -1;
string inputDirectory = AppDomain.CurrentDomain.BaseDirectory;

FileInfo configFileInfo = new(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", ".config"));

IDictionary<string, string> config = new Dictionary<string, string>();
ConfigurationService configService = new();

try
{
    await HandleArgumentsAsync(args);

    if (showHelp)
    {
        ShowHelp();
        exitCode = 0;
    }
    else
    {
        DirectoryInfo directoryInfo = new(inputDirectory);

        if (!directoryInfo.Exists) throw new ArgumentException("Bad directory.");

        var csProjFiles = directoryInfo.GetFiles("*.csproj", SearchOption.AllDirectories);

        var fileCollection = new List<CodeProjectFile>();
        //var collection = new ProjectCollection();

        foreach (var file in csProjFiles)
        {
            fileCollection.Add(new CodeProjectFile(file));
        }

        SortedSet<CodeReference> codeReferenceSet = new();

        foreach (var item in fileCollection)
        {
            codeReferenceSet.Add(new CodeReference(item.Name, CodeReferenceType.Project, null, 0));

            int pos = 0;
            foreach (var projectRef in item.ProjectReferences)
            {
                codeReferenceSet.Add(new CodeReference(projectRef, CodeReferenceType.Project, null, pos++));
            }

            pos = 0;
            foreach (var nugetRef in item.PackageReferences)
            {
                codeReferenceSet.Add(new CodeReference(nugetRef.Package, CodeReferenceType.NuGet, nugetRef.Version, pos++));
            }
        }

        List<CodeReference> copy = new List<CodeReference>(codeReferenceSet);

        foreach (var item in copy)
        {
            var matchingItem = codeReferenceSet.FirstOrDefault(c => c.Name == item.Name && c.CodeReferenceType == item.CodeReferenceType);
            Debug.Assert(matchingItem != null);

            var matchingFileItem = fileCollection.FirstOrDefault(f => f.Name == item.Name);

            if (matchingFileItem != null)
            {
                foreach (var projectRef in matchingFileItem.ProjectReferences)
                {
                    var matchingRef = codeReferenceSet.FirstOrDefault(c => c.Name == projectRef);

                    if (matchingRef != null && !matchingItem.Children.Contains(matchingRef))
                    {
                        matchingItem.Children.Add(matchingRef);
                    }
                }

                foreach (var nugetRef in matchingFileItem.PackageReferences)
                {
                    var matchingRef = codeReferenceSet.FirstOrDefault(c => c.Name == nugetRef.Package &&
                        c.Version == new Shibusa.DevTools.CsProjects.Cli.Version(nugetRef.Version));

                    if (matchingRef != null && !matchingItem.Children.Contains(matchingRef))
                    {
                        matchingItem.Children.Add(matchingRef);
                    }
                }
            }
        }

        bool change;

        do
        {
            change = false;
            var projects = codeReferenceSet.Where(c => c.CodeReferenceType == CodeReferenceType.Project)
                .OrderBy(c => c.OrdinalPosition).ToArray();

            foreach (var project in projects.ToArray())
            {
                foreach (var child in project.Children.ToArray())
                {
                    if (project.OrdinalPosition <= child.OrdinalPosition)
                    {
                        var match = codeReferenceSet.FirstOrDefault(c => c.GetHashCode() == project.GetHashCode());
                        if (match != null)
                        {
                            match.OrdinalPosition = child.OrdinalPosition + 1;
                            change = true;
                        }
                    }
                }
            }
        }
        while (change);


        foreach (var item in codeReferenceSet.Where(c => c.CodeReferenceType == CodeReferenceType.Project)
            .OrderBy(c => c.OrdinalPosition))
        {
            foreach (var child in item.Children.OrderBy(c => c.CodeReferenceType).ThenBy(c => c.OrdinalPosition))
            {
                Console.WriteLine($"\t{child}");
            }
        }

        Console.WriteLine();

        foreach (var project in codeReferenceSet.Where(c => c.CodeReferenceType == CodeReferenceType.Project &&
            c.Children.Count > 1).OrderBy(c => c.OrdinalPosition))
        {
            foreach (var child in project.Children)
            {
                var siblings = project.Children.Except(new[] { child });
                foreach (var sibling in siblings)
                {
                    if (sibling.ContainsInChain(child.Name))
                    {
                        Console.WriteLine($"{project}: Remove ref to {child}");
                    }
                }
            }
        }
    }

    exitCode = 0;
}
catch (Exception exc)
{
    exitCode = exc is ArgumentException ? -2 : -3;
    ShowHelp(exc.Message);
}
finally
{
    if (exitCode != 0) { Console.WriteLine($"Exited with code {exitCode}."); }
    Environment.Exit(exitCode);
}

async Task HandleArgumentsAsync(string[] args)
{
    int pos = Array.IndexOf(args, "--config-file");
    if (pos < 0)
    {
        config = (await ConfigurationService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.CsProjects];
    }
    else
    {
        if (pos == args.Length - 1) { throw new ArgumentException($"Expected file name after {args[pos]}"); }
        configFileInfo = new FileInfo(args[pos + 1]);
        config = (await ConfigurationService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.CsProjects];
    }

    for (int a = 0; a < args.Length; a++)
    {
        string argument = args[a].ToLower();

        switch (argument)
        {
            case "--help":
            case "-h":
            case "-?":
            case "?":
                showHelp = true;
                break;
            case "--directory":
            case "--dir":
            case "-d":
                if (a > args.Length - 1) { throw new ArgumentException($"Expecting a directory after {args[a]}"); }
                inputDirectory = args[++a];
                break;
            case "--config-file":
                a++;
                break;
            default:
                if (a == 0)
                {
                    inputDirectory = args[a];
                }
                else
                {
                    throw new ArgumentException($"'{args[a]}' is an unknown argument.");
                }
                break;
        }
    }
}

void ShowHelp(string? message = null)
{
    if (!string.IsNullOrWhiteSpace(message))
    {
        Console.WriteLine(message);
    }

    Dictionary<string, string> helpDefinitions = new()
    {
        { "(-d|--directory|--dir) <directory>", "The directory to search." },
        { "[-h|--help|?|-?]", "Show this help." }
    };

    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "devtools-csproj";

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }
}