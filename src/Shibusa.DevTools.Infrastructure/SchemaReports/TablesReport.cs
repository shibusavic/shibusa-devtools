using Shibusa.DevTools.Infrastructure.Schemas;

namespace Shibusa.DevTools.Infrastructure.SchemaReports
{
    /// <summary>
    /// Represents a report on tables.
    /// </summary>
    public sealed class TablesReport : ReportBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TablesReport"/> class.
        /// </summary>
        /// <param name="database">The <see cref="Database"/>.</param>
        /// <param name="directoryName">The output directory name.</param>
        /// <param name="overwriteFiles">An indicator of whether to overwrite files when they exist.</param>
        public TablesReport(Database database, string directoryName, bool overwriteFiles = false)
            : base(database, directoryName, overwriteFiles)
        { }

        /// <summary>
        /// Generate the report.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public override async Task GenerateAsync()
        {
            using Stream stream = CreateStream($"{directoryName}\\{CleanupDbName(database.Name)}_Tables.csv");

            string line = $"Schema,Table,Position,Column,Data Type,Precision,Max Length,Is Nullable,Default";

            await WriteLineToStreamAsync(stream, line);

            foreach (var table in database.Tables.OrderBy(t => t.FullName))
            {
                foreach (var column in table.Columns.OrderBy(c => c.Key))
                {
                    var col = column.Value;
                    line = $"{table.Schema},{table.Name},{col.OrdinalPosition},{col.Name},{col.DataType},{col.NumericPrecision},{col.MaxLength},{col.IsNullable},{col.ColumnDefault}";
                    await WriteLineToStreamAsync(stream, line);
                }
            }

            await CloseStreamAsync(stream);
        }
    }
}
