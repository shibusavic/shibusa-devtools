using Shibusa.DevTools.Infrastructure.Schemas;

namespace Shibusa.DevTools.Infrastructure.SchemaReports
{
    /// <summary>
    /// Represents a report on views.
    /// </summary>
    public sealed class ViewsReport : ReportBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ViewsReport"/> class.
        /// </summary>
        /// <param name="database">The <see cref="Database"/>.</param>
        /// <param name="directoryName">The output directory name.</param>
        /// <param name="overwriteFiles">An indicator of whether to overwrite files when they exist.</param>
        public ViewsReport(Database database, string directoryName, bool overwriteFiles = false)
            : base(database, directoryName, overwriteFiles)
        { }

        /// <summary>
        /// Generate the report.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public override async Task GenerateAsync()
        {
            using Stream stream = CreateStream($"{directoryName}\\{CleanupDbName(database.Name)}_Views.csv");

            string line = $"Schema,View,Definition";

            await WriteLineToStreamAsync(stream, line);

            foreach (var view in database.Views.OrderBy(v => v.FullName))
            {
                var def = view.Definition.Length < 50 ? view.Definition.Replace(Environment.NewLine, " ") : view.Definition.Substring(0, 50).Replace(Environment.NewLine, " ");
                line = $"{view.Schema},{view.Name},{def}";
                await WriteLineToStreamAsync(stream, line);
            }

            await CloseStreamAsync(stream);
        }
    }
}
