using Shibusa.DevTools.CsProjects.Cli;
using System.Diagnostics;
using System.Reflection;

bool showHelp = false;
int exitCode = -1;
string inputDirectory = AppDomain.CurrentDomain.BaseDirectory;

try
{
    HandleArguments(args);

    if (showHelp)
    {
        ShowHelp();
        exitCode = 0;
    }
    else
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(inputDirectory);

        if (!directoryInfo.Exists) throw new ArgumentException("Bad directory.");

        var csProjFiles = directoryInfo.GetFiles("*.csproj", SearchOption.AllDirectories);

        var collection = new ProjectCollection();

        foreach (var file in csProjFiles)
        {
            collection.Files.Add(new CodeProjectFile(file));
            collection.Files.Add(new CodeProjectFile(file)); // testing equality here
        }

        Debug.Assert(collection.Files.Count == csProjFiles.Length); // testing equality here

        foreach (var item in collection.Files)
        {
            Console.WriteLine(item.ToString());
            Console.WriteLine("\tPackage References");
            foreach (var r in item.PackageReferences)
            {
                Console.WriteLine($"\t\tnuget: {r.Ref}, {r.Version}");
            }

            Console.WriteLine("\tProject References");
            foreach (var r in item.ProjectReferences)
            {
                Console.WriteLine($"\t\t{r}");
            }
            Console.WriteLine();
        }

        exitCode = 0;
    }
}
catch (Exception exc)
{
    if (exc is ArgumentException)
    {
        exitCode = -2;
        ShowHelp(exc.Message);
    }
    else
    {
        exitCode = -3;
        ShowHelp(exc.ToString());
    }
}
finally
{
    if (exitCode != 0) { Console.WriteLine($"Exited with code {exitCode}."); }
    Environment.Exit(exitCode);
}

void HandleArguments(string[] args)
{
    for (int a = 0; a < args.Length; a++)
    {
        string argument = args[a].ToString();

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

    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "devtools-find-text";

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }
}