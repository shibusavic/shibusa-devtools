using Shibusa.DevTools.Infrastructure.Schemas;

namespace Shibusa.DevTools.Infrastructure.SchemaReports
{
    /// <summary>
    /// Represents a report on routines.
    /// </summary>
    public sealed class RoutinesReport : ReportBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RoutinesReport"/> class.
        /// </summary>
        /// <param name="database">The <see cref="Database"/>.</param>
        /// <param name="directoryName">The output directory name.</param>
        /// <param name="overwriteFiles">An indicator of whether to overwrite files when they exist.</param>
        public RoutinesReport(Database database, string directoryName, bool overwriteFiles = false)
            : base(database, directoryName, overwriteFiles)
        { }

        /// <summary>
        /// Generate the report.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public override async Task GenerateAsync()
        {
            using Stream stream = CreateStream($"{directoryName}\\{CleanupDbName(database.Name)}_Routines.csv");

            string line = $"Schema,Routine,Definition";

            await WriteLineToStreamAsync(stream, line);

            foreach (var routine in database.Routines.OrderBy(v => v.FullName))
            {
                var def = routine.Definition.Length < 50 ? routine.Definition.Replace(Environment.NewLine, " ") : routine.Definition.Substring(0, 50).Replace(Environment.NewLine, " ");
                line = $"{routine.Schema},{routine.Name},{def}";
                await WriteLineToStreamAsync(stream, line);
            }

            await CloseStreamAsync(stream);
        }
    }
}
