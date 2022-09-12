using Shibusa.DevTools.AppServices;
using Shibusa.DevTools.Infrastructure.SchemaReports;
using Shibusa.DevTools.Infrastructure.Schemas;
using System.Reflection;

string newline = Environment.NewLine;
bool showHelp = false;
int exitCode = -1;
string? outputDirectory = null;
bool overwriteFiles = false;
string? connectionString = null;

FileInfo configFileInfo = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", ".config"));
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
        Validate();

        var db = await DatabaseFactory.CreateAsync(connectionString!);

        await GenerateReportAsync<DependencyReport>(db, outputDirectory!, overwriteFiles);
        await GenerateReportAsync<TablesReport>(db, outputDirectory!, overwriteFiles);
        await GenerateReportAsync<ViewsReport>(db, outputDirectory!, overwriteFiles);
        await GenerateReportAsync<RoutinesReport>(db, outputDirectory!, overwriteFiles);

        exitCode = 0;
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

async Task GenerateReportAsync<T>(Database database, string directoryName, bool overwriteFiles) where T : ReportBase
{
    T? report = (T?)Activator.CreateInstance(typeof(T), database, directoryName, overwriteFiles);
    if (report != null)
    {
        await report.GenerateAsync();
    }
}

async Task HandleArgumentsAsync(string[] args)
{
    int pos = Array.IndexOf(args, "--config-file");
    if (pos < 0)
    {
        config = (await ConfigurationService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.Sql];
    }
    else
    {
        if (pos == args.Length - 1) { throw new ArgumentException($"Expected file name after {args[pos]}"); }
        configFileInfo = new FileInfo(args[pos + 1]);
        config = (await ConfigurationService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.Sql];
    }

    for (int a = 0; a < args.Length; a++)
    {
        string argument = args[a].ToLower();

        switch (argument)
        {
            case "--connection-string":
            case "-c":
                if (a >= args.Length - 1) { throw new ArgumentException($"Expecting a connection string after {args[a]}"); }
                connectionString = args[++a];
                break;
            case "--output-directory":
            case "-d":
                if (a >= args.Length - 1) { throw new ArgumentException($"Expecting a directory after {args[a]}"); }
                outputDirectory = args[++a];
                break;
            case "--overwrite":
            case "-o":
                overwriteFiles = true;
                break;
            case "--config-file":
                a++;
                break;
            case "--help":
            case "-h":
            case "?":
                showHelp = true;
                break;
            default:
                throw new ArgumentException($"Unknown argument: {args[a]}");
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
        { "{--connection-string | -c} <connection string>","Define the connection string." },
        { "{--output-directory | -d} <directory>]","Define the output directory." },
        { "[--overwrite | -o]","Overwrite output files if they exists." },
        { "[-h|--help|?|-?]", "Show this help." }
    };

    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "devtools-sql";

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }

    Console.WriteLine($"{newline}{newline}Usages:{newline}");
    Console.WriteLine($"To generate reports for a given database:");
    Console.WriteLine($"\t{assemblyName} -d /c/temp/db -c \"connection string\"");
    Console.WriteLine($"{Environment.NewLine}To ensure created files are overwritten:");
    Console.WriteLine($"\t{assemblyName} -d /c/temp/db -c \"connection string\" -o");
}

void Validate()
{
    if (string.IsNullOrWhiteSpace(connectionString)) { throw new ArgumentException("Connection string is required. Use -c."); }
    if (string.IsNullOrWhiteSpace(outputDirectory)) { throw new ArgumentException("Output directory is required. Use -d."); }

    while (outputDirectory.EndsWith("/") || outputDirectory.EndsWith("\\"))
    {
        outputDirectory = outputDirectory[0..^1];
    }

    if (!Directory.Exists(outputDirectory))
    {
        Directory.CreateDirectory(outputDirectory);
    }
}
