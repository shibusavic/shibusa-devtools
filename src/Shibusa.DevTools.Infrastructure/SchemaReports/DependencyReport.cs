using Shibusa.DevTools.Infrastructure.Schemas;
using System.Text;

namespace Shibusa.DevTools.Infrastructure.SchemaReports
{
    /// <summary>
    /// Represents a report on database object dependencies.
    /// </summary>
    public sealed class DependencyReport : ReportBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DependencyReport"/> class.
        /// </summary>
        /// <param name="database">The <see cref="Database"/>.</param>
        /// <param name="directoryName">The output directory name.</param>
        /// <param name="overwriteFiles">An indicator of whether to overwrite files when they exist.</param>
        public DependencyReport(Database database, string directoryName, bool overwriteFiles = false)
            : base(database, directoryName, overwriteFiles)
        { }

        /// <summary>
        /// Generate the report.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public override async Task GenerateAsync()
        {
            using Stream stream = CreateStream($"{directoryName}\\{CleanupDbName(database.Name)}_Dependency.txt");

            var sortedTables = database.GetTablesSortedByDependency();

            foreach (var table in sortedTables)
            {
                StringBuilder tableInfo = new StringBuilder();
                tableInfo.AppendLine($"{table.Schema}.{table.Name}");

                var tableDependencies = database.GetChildForeignKeysForTable(table);
                var viewDependencies = database.GetViewsReferencingTable(table);
                var routineDependencies = database.GetRoutinesReferencingTable(table);

                if (tableDependencies.Any())
                {
                    tableInfo.AppendLine("\tTable Dependencies");
                    foreach (var child in tableDependencies.Select(t => t.ChildTable))
                    {
                        tableInfo.AppendLine($"\t\t{child.FullName}");
                    }
                }

                if (viewDependencies.Any())
                {
                    tableInfo.AppendLine("\tView Dependencies");
                    foreach (var child in viewDependencies)
                    {
                        tableInfo.AppendLine($"\t\t{child.FullName}");
                    }
                }

                if (routineDependencies.Any())
                {
                    tableInfo.AppendLine("\tRoutine Dependencies");
                    foreach (var child in routineDependencies)
                    {
                        tableInfo.AppendLine($"\t\t{child.FullName}");
                    }
                }

                await WriteToStreamAsync(stream, tableInfo.ToString(), true);
            }

            await CloseStreamAsync(stream);
        }
    }
}
