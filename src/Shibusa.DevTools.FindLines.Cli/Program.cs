using Shibusa.DevTools.AppServices;
using System.Reflection;
using System.Text.RegularExpressions;

string expression = "";
HashSet<string> extensions = new();
string directory = ".";
bool caseInsensitive = false;
bool showHelp = false;
bool showLines = true;
bool trimLines = true;
bool showLineNumbers = false;
bool forceExpression = false;
bool prefixFilenameWithNewline = true;
bool useSingleline = false;
RegexOptions regexOptions = RegexOptions.Multiline;
DirectoryInfo dirInfo;
SearchOption searchOption = SearchOption.AllDirectories;
int exitCode = -1;

FileInfo configFileInfo = new(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", ".config"));

IDictionary<string, string> config = new Dictionary<string, string>();

try
{
    await HandleArgumentsAsync(args);

    if (showHelp)
    {
        ShowHelp();
    }
    else
    {
        ValidateConfig();
        Configure();

        IEnumerable<FileInfo> files;

        if (extensions.Any())
        {
            files = dirInfo.GetFiles("*", searchOption).Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
        }
        else
        {
            files = dirInfo.GetFiles("*", searchOption);
        }

        if (files.Any())
        {
            foreach (var file in files)
            {
                ProcessFile(file);
            }
        }
        else
        {
            Console.WriteLine("No files matched your extension configuration or no file were present to find.");
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

void ProcessFile(FileInfo file)
{
    string text = File.ReadAllText(file.FullName);

    var lines = new List<string>();
    var regex = new Regex(expression, regexOptions);
    MatchCollection matches = regex.Matches(text);
    if (matches.Any())
    {
        foreach (Match match in matches.Cast<Match>())
        {
            string line = match.Groups[0].Value;
            if (!regex.IsMatch(line))
            {
                line = string.Empty;
                break;
            }
            if (!string.IsNullOrWhiteSpace(line))
            {
                if (trimLines) { line = line.Trim(); }
                lines.Add(line);
            }
        }
    }

    if (lines.Any())
    {
        string prefix = prefixFilenameWithNewline ? Environment.NewLine : string.Empty;
        Console.WriteLine($"{prefix}{file.FullName}");

        if (showLineNumbers && !useSingleline)
        {
            ShowLinesWithLineNumbers(file, lines);
        }
        else if (showLines)
        {
            foreach (string line in lines)
            {
                Console.WriteLine(line);
            }
        }
    }
}

void ShowLinesWithLineNumbers(FileInfo fileInfo, IEnumerable<string> matchedLines)
{
    var lines = File.ReadAllLines(fileInfo.FullName);
    int matchedLineCounter = 0;
    int count = matchedLines.Count();
    int lineNumber = 0;
    foreach (string line in lines)
    {
        lineNumber++;
        if (matchedLines.Select(l => l.Trim()).Contains(line.Trim()))
        {
            matchedLineCounter++;
            string lineToShow = trimLines ? line.Trim() : line;
            Console.WriteLine($"{lineNumber}\t{lineToShow}");

            // once we've found all the expected matches, get out.
            if (matchedLineCounter == count) { break; }
        }
    }
}

string AddExpression(string expression)
{
    const string prefix = "^.+?";
    const string suffix = ".+?$";

    if (config.ContainsKey(expression)) { return config[expression]; }

    if (forceExpression || (expression.StartsWith(prefix.First()) && expression.EndsWith(suffix.Last())))
    {
        return expression;
    }
    else if (expression.StartsWith(prefix.First()))
    {
        return AddExpression($"{expression}{suffix}");
    }
    else if (expression.EndsWith(suffix.Last()))
    {
        return AddExpression($"{prefix}{expression}");
    }
    else
    {
        return AddExpression($"{prefix}{expression}{suffix}");
    }
}

void AddExtension(string extension)
{
    if (extension.Contains(','))
    {
        var split = extension.Split(",", StringSplitOptions.RemoveEmptyEntries);
        foreach (var s in split)
        {
            AddSingleExtension(s);
        }
    }
    else
    {
        AddSingleExtension(extension);
    }
}

void AddSingleExtension(string extension)
{
    while (extension.StartsWith(".") && extension.Length > 1)
    {
        extension = extension[1..];
    }

    if (!string.IsNullOrWhiteSpace(extension))
    {
        extensions.Add($".{extension}");
    }
}

async Task HandleArgumentsAsync(string[] args)
{
    int pos = Array.IndexOf(args, "--config-file");
    if (pos < 0)
    {
        config = (await ConfigurationService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.FindLines];
    }
    else
    {
        if (pos == args.Length - 1) { throw new ArgumentException($"Expected file name after {args[pos]}"); }
        configFileInfo = new FileInfo(args[pos + 1]);
        config = (await ConfigurationService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.FindLines];
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
                directory = args[++a];
                break;
            case "--expression":
            case "-e":
                if (a > args.Length - 1) { throw new ArgumentException($"Expecting an expression after {args[a]}"); }
                expression = AddExpression(args[++a]);
                break;
            case "--insensitive":
            case "-i":
                caseInsensitive = true;
                break;
            case "--extension":
            case "-x":
                if (a > args.Length - 1) { throw new ArgumentException($"Expecting an extension after {args[a]}"); }
                AddExtension(args[++a]);
                break;
            case "--not-recursive":
                searchOption = SearchOption.TopDirectoryOnly;
                break;
            case "--names-only":
                showLines = false;
                break;
            case "--show-line-numbers":
            case "-ln":
                showLineNumbers = true;
                break;
            case "--no-trim":
                trimLines = false;
                break;
            case "--force":
            case "-f":
                forceExpression = true;
                break;
            case "--squash":
                prefixFilenameWithNewline = false;
                break;
            case "--use-singleline":
                useSingleline = true;
                break;
            case "--config-file":
                a++;
                break;
            default:
                if (a == 0)
                {
                    directory = args[a];
                }
                else if (a == 1)
                {
                    expression = AddExpression(args[a]);
                }
                else
                {
                    throw new ArgumentException($"'{args[a]}' is an unknown argument.");
                }
                break;
        }

        if (!string.IsNullOrWhiteSpace(directory) && config.ContainsKey(directory))
        {
            directory = config[directory!];
        }
    }
}

void Configure()
{
    if (useSingleline) { regexOptions = RegexOptions.Singleline; }
    if (caseInsensitive) { regexOptions |= RegexOptions.IgnoreCase; }
    if (showLineNumbers) { showLines = true; }

    dirInfo = new DirectoryInfo(directory);
}

void ValidateConfig()
{
    if (string.IsNullOrWhiteSpace(expression))
    {
        throw new ArgumentException("An expression is requred; use -e.");
    }

    if (string.IsNullOrWhiteSpace(directory))
    {
        throw new ArgumentException("No directory was provided");
    }

    if (!Directory.Exists(directory))
    {
        throw new ArgumentException($"{directory} does not exist.");
    }

    if (showLineNumbers && useSingleline)
    {
        Console.WriteLine("WARNING: showing line numbers with the single-line option is not possible.");
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
        { "-d|--directory|--dir <directory>", "The directory to search." },
        { "-e|--expression <expression>", "A regular expression by which to search." },
        { "[-x|--extension <file extension>]", "Add file extension to extensions searched. When no extensions are provided, all files are searched." },
        { "[-f|--force]", "Force your expression to be accepted without manipulation." },
        { "[-i|--insensitive]", "Make search case insensitive." },
        { "[-ln|--show-line-numbers]", "Show the line numbers for matching lines." },
        { "[--not-recursive]", "Limit file searching to the top directory only." },
        { "[--no-trim]", "Do not trim the matching lines in the output." },
        { "[--squash]","Prevent creation of blank lines before each file name in the output."},
        { "[--names-only]", "Show only file names." },
        { "[--use-singleline]", "Use Singleline (instead of the default Multiline) for regular expressions." },
        { "[--config-file <path>]","Use specified configuration file. Passed by default from CLI caller."},
        { "[-h|--help|?|-?]", "Show this help." }
    };

    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "devtools-find-lines";

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }
}