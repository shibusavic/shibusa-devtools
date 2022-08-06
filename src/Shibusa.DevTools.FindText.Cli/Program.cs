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
SearchOption searchOption = SearchOption.TopDirectoryOnly;
int exitCode = -1;

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

void ProcessFile(FileInfo file)
{
    string text = File.ReadAllText(file.FullName);

    var lines = new List<string>();
    var regex = new Regex(expression);
    MatchCollection matches = regex.Matches(text);
    if (matches.Any())
    {
        foreach (Match match in matches)
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
    if (forceExpression || (expression.StartsWith("^") && expression.EndsWith("$")))
    {
        return expression;
    }
    else if (expression.StartsWith("^"))
    {
        return AddExpression($"{expression}.+?$");
    }
    else if (expression.EndsWith("$"))
    {
        return AddExpression($"^.+?{expression}");
    }
    else
    {
        return AddExpression($"^.+?{expression}.+?$");
    }
}

void AddExtension(string extension)
{
    if (extension.Contains(","))
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
            case "--recursive":
            case "-r":
                searchOption = SearchOption.AllDirectories;
                break;
            case "--names-only":
                showLines = false;
                break;
            case "--show-line-numbers":
            case "-ln":
                showLineNumbers = true;
                break;
            case "--do-not-trim":
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
            default:
                if (a == 0)
                {
                    directory = args[a];
                }
                else if (a == 1)
                {
                    expression = args[a];
                }
                else
                {
                    throw new ArgumentException($"'{args[a]}' is an unknown argument.");
                }
                break;
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
        { "[-r|--recursive]", "Make file searching include subdirectories." },
        { "[-ln|--show-line-numbers]", "Show the line numbers for matching lines." },
        { "[--do-not-trim]", "Do not trim the matching lines in the output." },
        { "[--squash]","Prevent creation of blank lines before each file name in the output."},
        { "[--names-only]", "Show only file names." },
        { "[--use-singleline]", "Use Singleline (instead of the default Multiline) for regular expressions." },
        { "[-h|--help|?|-?]", "Show this help." }
    };

    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "devtools-find-text";

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }

    //Console.WriteLine($"{Environment.NewLine}Examples:");
    //Console.WriteLine($"{Environment.NewLine}Find any file containing the whole word 'the' - case-sensitive search:");
    //Console.WriteLine($"\t{assemblyName} -d \"/c/repos\" -e \"\\bthe\\b\"");

    //Console.WriteLine($"{Environment.NewLine}Same search, but case insensitive:");
    //Console.WriteLine($"\t{assemblyName} - \"/c/repos\" -e \"\\bthe\\b\" -i");

    //Console.WriteLine($"{Environment.NewLine}Same search, but case insensitive and searching subdirectories:");
    //Console.WriteLine($"\t{assemblyName} -d \"/c/repos\" -e \"\\bthe\\b\" -i -r");

    //Console.WriteLine($"{Environment.NewLine}Shows lines:");
    //Console.WriteLine($"\t{assemblyName} -d \"/c/repos\" -e \"\\bthe\\b\" -i -r -l");

    //Console.WriteLine($"{Environment.NewLine}Shows lines with line numbers:");
    //Console.WriteLine($"\t{assemblyName} -d \"/c/repos\" -e \"\\bthe\\b\" -i -r -ln");

    //Console.WriteLine($"equivalent to:");
    //Console.WriteLine($"\t{assemblyName} -d \"/c/repos\" -e \"\\bthe\\b\" -i -r -ln -l");

    //Console.WriteLine($"{Environment.NewLine}Trim the lines in the output:");
    //Console.WriteLine($"\t{assemblyName} -d \"/c/repos\" -e \"\\bthe\\b\" -i -r -ln -t");

    //Console.WriteLine($"{Environment.NewLine}Force your expression (to avoid full-line-capturing manipulation):");
    //Console.WriteLine($"\t{assemblyName} -d \"/c/repos\" -e \"First Name: [a-zA-Z]+\" -r -ln -f");

    //Console.WriteLine($"{Environment.NewLine}Find a multi-line block of text (by switching to single-line):");
    //Console.WriteLine($"\t\t(It may seem counter-intuitive, but you use the single-line regular expression option to accomplish this;");
    //Console.WriteLine("\t\t see: https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options.)");
    //Console.WriteLine($"\t{assemblyName} -d \"/c/repos\" -e \"<Id>.*</Id>\" -s -r -f");
}