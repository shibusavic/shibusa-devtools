using Shibusa.DevTools.AppServices;
using System.Reflection;

bool showHelp = false;
string NL = Environment.NewLine;

int exitCode = -1;

FileInfo configFileInfo = new(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", ".config"));

IDictionary<string, Dictionary<string, string>> configDictionary = new Dictionary<string, Dictionary<string, string>>();

ConfigAction action = ConfigAction.None;
string? commandKey = null;
string? aliasKey = null;
string? aliasValue = null;
string? filterKey = null;

try
{
    await HandleArgumentsAsync(args);

    if (showHelp)
    {
        ShowHelp();
    }
    else
    {
#if DEBUG
        configDictionary[ConfigurationService.Keys.Sql]["test"] = "http://test.com";
#endif
        if (action == ConfigAction.Show)
        {
            Console.WriteLine($"{Environment.NewLine}Config Keys Prefixes: {string.Join(" | ", configDictionary.Keys)}");

            Console.WriteLine($"{Environment.NewLine}Aliases{Environment.NewLine}--------------------------------------");

            foreach (var key in configDictionary.Keys)
            {
                if (filterKey == null || filterKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var alias in configDictionary[key].Keys)
                    {
                        string column1 = $"{key}.{alias}".PadRight(20);
                        Console.WriteLine($"{column1}\t{configDictionary[key][alias]}");
                    }
                }
            }
            WriteExamples();
        }

        if (action == ConfigAction.Add)
        {
            if (commandKey != null && aliasKey != null && aliasValue != null)
            {
                configDictionary[commandKey][aliasKey] = aliasValue;
                await ConfigurationService.SaveConfigurationFileAsync(configDictionary, configFileInfo);
            }
        }

        if (action == ConfigAction.Delete)
        {
            if (commandKey != null && aliasKey != null && configDictionary[commandKey].ContainsKey(aliasKey))
            {
                configDictionary[commandKey].Remove(aliasKey);
                await ConfigurationService.SaveConfigurationFileAsync(configDictionary, configFileInfo);
            }
        }
    }

    exitCode= 0;
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
    string[] arguments = args.Select(a => a.ToLower()).ToArray();

    int pos = Array.IndexOf(arguments, "--config-file");

    if (pos < 0)
    {
        configDictionary = await ConfigurationService.GetConfigurationAsync(configFileInfo);
    }
    else
    {
        if (pos == arguments.Length - 1) { throw new ArgumentException($"Expected file name after {args[pos]}"); }
        configFileInfo = new FileInfo(arguments[pos + 1]);
        configDictionary = await ConfigurationService.GetConfigurationAsync(configFileInfo);
    }

    arguments = args[..pos];

    for (int a = 0; a < arguments.Length; a++)
    {
        switch (arguments[a])
        {
            case "--help":
            case "-h":
            case "-?":
            case "?":
                showHelp = true;
                break;
            case "show":
                if (a < arguments.Length - 1)
                {
                    filterKey = arguments[++a];
                }
                action = ConfigAction.Show;
                break;
            case "--config-file":
                a++;
                break;
            default:
                // ignore
                break;
        }

        if (showHelp || action == ConfigAction.Show) { break; }

        if (action == ConfigAction.None && a < arguments.Length - 1)
        {
            action = GetConfigAction(arguments[a]);
            continue;
        }

        if (aliasKey == null)
        {
            if (action != ConfigAction.None)
            {
                if (a > args.Length - 1) { throw new ArgumentException($"Expecting a value after {args[a]}"); }
                (commandKey, aliasKey) = GetAliasKeys(args[a]);
                if (action == ConfigAction.Add)
                {
                    aliasValue = args[++a];
                }
            }
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
        { "[show [<key>]]", "Show the key/value pairs; use <key> to filter."},
        { "[add <key> <value>]","Add a key/value pair to the config."},
        { "[(delete|remove) <key>]","Remove a key/value pair from the config."},
        { "[--config-file <path>]","Use specified configuration file. Passed by default from CLI caller."},
        { "[-h|--help|?|-?]", "Show this help." }
    };

    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "devtools-config";

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{NL}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{NL}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }
    WriteExamples();
}

void WriteExamples()
{
    Console.WriteLine($"{NL}To add an alias: devtools config add fl.test \"this is my value\"");
}

(string Key, string AliasKey) GetAliasKeys(string alias)
{
    var split = alias.Split(".", StringSplitOptions.RemoveEmptyEntries);
    if (split.Length != 2) { throw new ArgumentException($"Could not parse alias: {alias}"); }
    return (split[0].ToLower(), split[1].ToLower());
}

ConfigAction GetConfigAction(string action) => action.ToLower() switch
{
    "show" => ConfigAction.Show,
    "add" => ConfigAction.Add,
    "remove" => ConfigAction.Delete,
    "delete" => ConfigAction.Delete,
    _ => ConfigAction.None
};

enum ConfigAction
{
    None = 0,
    Show,
    Add,
    Delete
}