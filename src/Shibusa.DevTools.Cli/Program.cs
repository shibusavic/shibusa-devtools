using System.Diagnostics;
using System.Reflection;

const string findTextCommandKey = "ft";
const string csProjCommandKey = "cs";

Dictionary<string, string> subcommandDictionary = new(StringComparer.CurrentCultureIgnoreCase) {
    { findTextCommandKey, "devtools-find-text.exe"},
    { csProjCommandKey, "devtools-csproj.exe"}
};

bool showHelp = false;
string? subcommand = null;
int exitCode = 1;

HandleArguments(args, out string[] childArgs);

if (args.Length == 0 || showHelp)
{
    if (subcommand == null)
    {
        ShowHelp();
        exitCode = 0;
    }
    else
    {
        var fileToExecute = GetSubcommandFileInfo(subcommand);

        if (fileToExecute == null) throw new ArgumentException("A valid subcommand is required.");

        if (subcommandDictionary.Keys.Contains(subcommand))
        {
            ProcessStartInfo process_start_info = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = fileToExecute.FullName,
                Arguments = childArgs.Any() ? string.Join(' ', childArgs) : "--help"
            };
            var process = Process.Start(process_start_info);
            process?.WaitForExit();
            exitCode = process?.ExitCode ?? 2;
        }
        else throw new ArgumentException("A valid subcommand is required.");
    }
}
else
{
    var fileToExecute = GetSubcommandFileInfo(subcommand);

    if (fileToExecute == null) throw new ArgumentException("A valid subcommand is required.");


    if (subcommandDictionary.Keys.Contains(subcommand))
    {
        ProcessStartInfo process_start_info = new()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = fileToExecute.FullName,
            Arguments = string.Join(' ', args[1..^0])
        };
        var process = Process.Start(process_start_info);
        process?.WaitForExit();
        exitCode = process?.ExitCode ?? 3;
    }
    else throw new ArgumentException("A valid subcommand is required.");
}

Environment.Exit(exitCode);

FileInfo? GetSubcommandFileInfo(string? subcommand)
{
    if (subcommand == null) return null;

    var dir = AppDomain.CurrentDomain.BaseDirectory;

#if DEBUG
    dir = @"c:\repos\shibusa-devtools\src";
#endif

    var exes = Directory.GetFiles(dir, "*.exe", SearchOption.AllDirectories)
        .Select(f => new FileInfo(f));

    return exes.FirstOrDefault(f => f.Name.Equals(subcommandDictionary[subcommand], StringComparison.InvariantCultureIgnoreCase));
}

void HandleArguments(string[] args, out string[] childArgs)
{
    childArgs = Array.Empty<string>();

    for (int a = 0; a < args.Length; a++)
    {
        string argument = args[a].ToLower();

        switch (argument)
        {
            case "help":
            case "--help":
            case "-h":
            case "-?":
            case "?":
                if (!showHelp && subcommand == null)
                {
                    if (a < args.Length - 1)
                    {
                        if (subcommandDictionary.ContainsKey(args[a + 1]))
                        {
                            subcommand ??= args[++a];
                        }
                    }
                    showHelp = true;
                }
                break;
            default:
                if (subcommandDictionary.ContainsKey(argument))
                {
                    subcommand ??= argument;
                    childArgs = args[++a..];
                }
                break;
        }

        if (!string.IsNullOrWhiteSpace(subcommand)) { break; } // If we hit a subcommand, we're done; everything that follows belongs to the subcommand.
    }
}

void ShowHelp(string? message = null)
{
    if (!string.IsNullOrWhiteSpace(message)) { Console.WriteLine(message); }

    string subcommands = "(" + string.Join(" | ", subcommandDictionary.Keys) + ")";
    string subcommandsExtended = $"{subcommands} <options>";

    Dictionary<string, string> helpDefinitions = new()
    {
        { subcommandsExtended, "Execute respective sub-command." },
        { $"[-h|-?|?|help|--help [{subcommands}]]", "Show respective sub-command's help." }
    };

    string? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }
}