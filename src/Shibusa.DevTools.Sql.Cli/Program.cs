using Shibusa.DevTools.AppServices;
using System.Reflection;

bool showHelp = false;
int exitCode = -1;

FileInfo configFileInfo = new FileInfo(".config");
IDictionary<string, string> config = new Dictionary<string, string>();
ConfigurationService configService = new ConfigurationService();

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

    }
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
        config = (await configService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.Sql];
    }
    else
    {
        if (pos == args.Length - 1) { throw new ArgumentException($"Expected file name after {args[pos]}"); }
        configFileInfo = new FileInfo(args[pos + 1]);
        config = (await configService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.Sql];
    }

    for (int a = 0; a < args.Length; a++)
    {
        string argument = args[a].ToLower();

        switch (argument)
        {
            case "--config-file":
                a++;
                break;
            case "--help":
            case "-h":
            case "-?":
            case "?":
                showHelp = true;
                break;
            default:
                throw new ArgumentException($"'{args[a]}' is an unknown argument.");
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
        { "[-h|--help|?|-?]", "Show this help." }
    };

    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "devtools-sql";

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }
}