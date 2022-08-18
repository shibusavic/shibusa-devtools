using Shibusa.DevTools.AppServices;
using System.Diagnostics;
using System.Reflection;

const string findLinesCommandKey = "fl";
const string csProjCommandKey = "cs";
const string sqlCommandKey = "sql";
const string configCommandKey = "config";

Dictionary<string, string> subcommandDictionary = new(StringComparer.InvariantCultureIgnoreCase);

if (IsWindows())
{
    subcommandDictionary.Add(findLinesCommandKey, "devtools-find-lines.exe");
    subcommandDictionary.Add(csProjCommandKey, "devtools-csproj.exe");
    subcommandDictionary.Add(sqlCommandKey, "devtools-sql.exe");
    subcommandDictionary.Add(configCommandKey, "devtools-config.exe");
}
else
{
    subcommandDictionary.Add(findLinesCommandKey, "devtools-find-lines");
    subcommandDictionary.Add(csProjCommandKey, "devtools-csproj");
    subcommandDictionary.Add(sqlCommandKey, "devtools-sql");
    subcommandDictionary.Add(configCommandKey, "devtools-config");
}

bool showHelp = false;
string? subcommand = null;
int exitCode = 1;

ConfigurationService configService = new();

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
                Arguments = childArgs.Any() ? GetChildArgsString(childArgs) : "--help"
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
            Arguments = GetChildArgsString(childArgs)
        };
        var process = Process.Start(process_start_info);
        process?.WaitForExit();
        exitCode = process?.ExitCode ?? 3;
    }
    else throw new ArgumentException("A valid subcommand is required.");
}

Environment.Exit(exitCode);

string GetChildArgsString(string[] args)
{
    List<string> childArgs = new();
    for (int a = 0; a < args.Length; a++)
    {
        if (args[a].Contains(" "))
        {
            childArgs.Add($"\"{args[a]}\"");
        }
        else
        {
            childArgs.Add(args[a]);
        }
    }
    return string.Join(" ", childArgs);
}

FileInfo? GetSubcommandFileInfo(string? subcommand)
{
    if (subcommand == null) return null;

    var dir = AppDomain.CurrentDomain.BaseDirectory;

#if DEBUG
    if (IsWindows())    
    {
        dir = @"c:\repos\shibusa-devtools\src";
    }
#endif

    var exes = Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
        .Where(f => subcommandDictionary.Values.Contains(new FileInfo(f).Name))
        .Select(f => new FileInfo(f));

    return exes.FirstOrDefault(f => f.Name.Equals(subcommandDictionary[subcommand], StringComparison.InvariantCultureIgnoreCase));
}

void HandleArguments(string[] args, out string[] childArgs)
{
    var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? String.Empty;

    List<string> argsToPass = new();

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
                            argsToPass.Add("--help");
                        }
                    }
                    showHelp = true;
                }
                break;
            default:
                if (subcommandDictionary.ContainsKey(argument))
                {
                    subcommand ??= argument;
                    argsToPass.AddRange(args[++a..]);
                }
                break;
        }

        if (!string.IsNullOrWhiteSpace(subcommand)) { break; } // If we hit a subcommand, we're done; everything that follows belongs to the subcommand.
    }

    FileInfo configFile = new FileInfo(Path.Combine(dir, ".config"));

    argsToPass.Add("--config-file");
    argsToPass.Add(configFile.FullName);

    childArgs = argsToPass.ToArray();
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

static bool IsWindows() => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);