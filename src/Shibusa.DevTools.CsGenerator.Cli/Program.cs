using Shibusa.Data.Abstractions;
using Shibusa.DevTools.AppServices;
using Shibusa.DevTools.Infrastructure.Abstractions;
using Shibusa.Extensions;
using System.Reflection;

bool showHelp = false;
int exitCode = -1;
string outputDirectory = Path.GetTempPath();
DirectoryInfo outputDirInfo = new(outputDirectory);
string? ns = "NamespaceName";

string? connectionString = null;
SortedSet<string> tables = new();
DatabaseEngine dbEngine = DatabaseEngine.None;
CodeGenerationConfiguration codeGenerationConfiguration = new();

FileInfo configFileInfo = new(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", ".config"));

IDictionary<string, string> config = new Dictionary<string, string>();

try
{
    await StartupAsync(args);

    if (showHelp)
    {
        ShowHelp();
    }
    else
    {
        IDatabaseFactory factory = dbEngine == DatabaseEngine.Postgres
            ? new Shibusa.DevTools.Infrastructure.PostgreSQL.DatabaseFactory()
            : throw new Exception("Only PostgreSQL is supported right now.");

        var database = await factory.CreateAsync(connectionString!, includeTables: true, false, false, false);

        CancellationTokenSource cts = new();

        Task<Task<string>>[] tasks = new Task<Task<string>>[database.Tables.Count];
        int taskIndex = 0;

        if (database != null)
        {
            if (tables.Any())
            {
                foreach (var table in tables)
                {
                    Console.WriteLine(database.Tables.FirstOrDefault(t => t.Name == table)?.Name);
                }
            }
            else
            {
                foreach (var table in database.Tables)
                {
                    tasks[taskIndex++] = Task.Factory.StartNew(() =>
                        CodeGenerationService.GenerateFromTableAsync(codeGenerationConfiguration, table));
                }
            }
        }

        var finalTask = Task.Factory.ContinueWhenAll(tasks, completedTasks =>
        {
            for (int i = 0; i < completedTasks.Length; i++)
            {
                Console.WriteLine(completedTasks.ElementAt(i).Result.Result);
                if (completedTasks.ElementAt(i).Exception != null)
                {
                    Console.WriteLine(completedTasks.ElementAt(i).Exception?.ToString());
                }
            }
        });

        finalTask.Wait();
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

async Task StartupAsync(string[] args)
{
    int pos = Array.IndexOf(args, "--config-file");
    if (pos < 0)
    {
        config = (await ConfigurationService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.CsGen];
    }
    else
    {
        if (pos == args.Length - 1) { throw new ArgumentException($"Expected file name after {args[pos]}"); }
        configFileInfo = new FileInfo(args[pos + 1]);
        config = (await ConfigurationService.GetConfigurationAsync(configFileInfo))[ConfigurationService.Keys.CsGen];
    }

    bool useStructs = false;
    bool useClasses = false;
    bool useFields = false;
    bool includeDbAttributes = false;
    bool usePropertyGetters = false;
    bool usePropertySetters = false;
    bool generateConstructor = false;

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
            case "--connection-string":
            case "--connection":
            case "-c":
                if (a > args.Length - 1) { throw new ArgumentException($"Expecting a connection string after {args[a]}"); }
                connectionString = args[++a];
                break;
            case "--output-directory":
            case "--out-dir":
            case "-o":
                if (a > args.Length - 1) { throw new ArgumentException($"Expecting a directory after {args[a]}"); }
                outputDirectory = args[++a];
                break;
            case "--tables":
            case "--table":
            case "-t":
                throw new ArgumentException($"The {args[a]} is not supported currently.");
            //if (a > args.Length - 1) { throw new ArgumentException($"Expecting one or more table names after {args[a]}; try '-t public.users,public.permissions'"); }
            //ProcessTables(args[++a]);
            //break;
            case "--database-engine":
            case "--db":
            case "-d":
                if (a > args.Length - 1) { throw new ArgumentException($"Expecting the name of a database engine after {args[a]}; try '-d PostgreSQL'"); }
                dbEngine = args[++a].GetEnum<DatabaseEngine>();
                if (dbEngine == DatabaseEngine.None)
                {
                    throw new ArgumentException($"Could not parse {args[a]} as a database engine.");
                }
                break;
            case "--namespace":
            case "--ns":
            case "-n":
                if (a > args.Length - 1) { throw new ArgumentException($"Expecting the namespace after {args[a]}; try '-n MyCompany.MyProject'"); }
                ns = args[++a];
                break;
            case "--use-structs":
            case "--structs":
            case "--struct":
                useStructs = true;
                break;
            case "--use-classes":
            case "--classes":
            case "--class":
                useClasses = true;
                break;
            case "--use-fields":
            case "--fields":
            case "--field":
                useFields = true;
                break;
            case "--include-attributes":
            case "--attributes":
            case "--attribute":
                includeDbAttributes = true;
                break;
            case "--use-property-getters":
            case "--property-getters":
                usePropertyGetters = true;
                break;
            case "--use-property-setters":
            case "--property-setters":
                usePropertySetters = true;
                break;
            case "--properties":
            case "--property":
                usePropertyGetters = true;
                usePropertySetters = true;
                break;
            case "--generate-constructor":
            case "--constructor":
                generateConstructor = true;
                break;
            case "--config-file":
                a++;
                break;
            default:
                throw new ArgumentException($"'{args[a]}' is an unknown argument.");
        }
    }

    if (dbEngine == DatabaseEngine.None)
    {
        dbEngine = DatabaseEngine.Postgres;
    }

    if (!string.IsNullOrWhiteSpace(connectionString) && config.ContainsKey(connectionString))
    {
        connectionString = config[connectionString!];
    }

    if (!string.IsNullOrWhiteSpace(outputDirectory) && config.ContainsKey(outputDirectory))
    {
        outputDirectory = config[outputDirectory!];
    }

    outputDirInfo = new(outputDirectory);

    if (!outputDirInfo.Exists)
    {
        outputDirInfo.Create();
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new ArgumentException($"Connection string is required; use -c \"<connection string>\"");
    }

    if (!useStructs && !useClasses) { useClasses = true; }
    if (!useFields && !usePropertyGetters && !usePropertySetters) { usePropertyGetters = true; }

    codeGenerationConfiguration = new(directoryInfo: outputDirInfo,
        useStructs, useClasses, useFields, includeDbAttributes, usePropertyGetters, usePropertySetters, generateConstructor, ns);
}

void ShowHelp(string? message = null)
{
    if (!string.IsNullOrWhiteSpace(message))
    {
        Console.WriteLine(message);
    }

    Dictionary<string, string> helpDefinitions = new()
    {
        { "-c | --connection-string | --connection <connection string>","Define the connection string." },
        { "-o | --output-directory | --out-dir <directory>]","Define the output directory." },
        { "-n | --ns | --namespace <namespace>","Set the C# namespace in the output files."},
        //{ "-t | --table[s] <comma separated table list>","Generate only the specified tables."},
        { "[-d | --db | --database-engine <name of engine>]","Provide engine for connection string."},
        { "[--struct[s] | --use-structs]","Generate structs instead of classes."},
        { "[--class[es] | --use-classes]","Generate classes. This is the default."},
        { "[--field[s] | --use-fields","Generate fields instead of properties."},
        { "[--property-getters | --use-property-getters","Generate properties with getters."},
        { "[--property-setters | --use-property-setters","Generate properties with setters."},
        { "[--properties","Generate properties with both getters and setters."},
        { "[--constructor | --generate-constructor","Generate a constructor."},
        { "[--attribute[s] | --include-attributes]","Add Data Schema attributes to the generated structs/classes."},
        { "[--config-file <path>]","Use specified configuration file. Passed by default from CLI caller."},
        { "[-h|--help|?|-?]", "Show this help." }
    };

    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "devtools-csgen";

    int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

    Console.WriteLine($"{Environment.NewLine}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{Environment.NewLine}");

    foreach (var helpItem in helpDefinitions)
    {
        Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
    }

    string NL = Environment.NewLine;

    Console.WriteLine($"{NL}List of database engines:{NL}");

    foreach (var engine in EnumExtensions.GetDescriptions<DatabaseEngine>().Where(e => e != DatabaseEngine.None.GetDescription()))
    {
        Console.WriteLine($"\t{engine}");
    }
}

//void ProcessTables(string tablesText)
//{
//    if (!string.IsNullOrWhiteSpace(tablesText))
//    {
//        var split = tablesText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
//        foreach (var table in split)
//        {
//            tables.Add(table);
//        }
//    }
//}
