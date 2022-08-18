using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Shibusa.DevTools.Infrastructure.Tests")]
namespace Shibusa.DevTools.Infrastructure.Schemas
{
    /// <summary>
    /// Represents a Microsoft Sql Server database structure.
    /// </summary>
    public sealed class Database
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="tables">A collection of <see cref="Table"/> instances.</param>
        /// <param name="foreignKeys">A collectoin of <see cref="ForeignKey"/> instances.</param>
        /// <param name="routines">A collection of <see cref="Routine"/> instances.</param>
        /// <param name="views">A collection of <see cref="View"/> instances.</param>
        /// <param name="connectionString">The connection string.</param>
        internal Database(string name,
            IEnumerable<Table>? tables,
            IEnumerable<ForeignKey>? foreignKeys,
            IEnumerable<Routine>? routines,
            IEnumerable<View>? views,
            string? connectionString = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
            Tables = new ReadOnlyCollection<Table>(tables?.ToList() ?? new List<Table>());
            ForeignKeys = new ReadOnlyCollection<ForeignKey>(foreignKeys?.ToList() ?? new List<ForeignKey>());
            Routines = new ReadOnlyCollection<Routine>(routines?.ToList() ?? new List<Routine>());
            Views = new ReadOnlyCollection<View>(views?.ToList() ?? new List<View>());
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string? ConnectionString { get; }

        /// <summary>
        /// Gets a read-only collection of <see cref="Table"/> objects.
        /// </summary>
        public IReadOnlyCollection<Table> Tables { get; }

        /// <summary>
        /// Gets a read-only collection of <see cref="ForeignKey"/> objects.
        /// </summary>
        public IReadOnlyCollection<ForeignKey> ForeignKeys { get; }

        /// <summary>
        /// Gets a read-only collection of <see cref="Routine"/> objects.
        /// </summary>
        public IReadOnlyCollection<Routine> Routines { get; }

        /// <summary>
        /// Gets a read-only collection of <see cref="View"/> objects.
        /// </summary>
        public IReadOnlyCollection<View> Views { get; }

        /// <summary>
        /// Gets a collection of <see cref="ForeignKey"/> objects where
        /// the provided table is the parent.
        /// </summary>
        /// <param name="table">The parent table to evalutate.</param>
        /// <returns>A collection of <see cref="ForeignKey"/> objects.</returns>
        public IEnumerable<ForeignKey> GetChildForeignKeysForTable(Table table)
        {
            return ForeignKeys.Where(f => f.ParentTable.Equals(table));
        }

        /// <summary>
        /// Get a collection of <see cref="View"/> objects that MAY contain references
        /// to the provided table.
        /// </summary>
        /// <param name="table">The table for which to search.</param>
        /// <returns>A collection of distinct <see cref="View"/> objects.</returns>
        /// <remarks>This method uses a regular expression to find references; it may
        /// inadvertently match on comments or strings containing the table name.</remarks>
        public IEnumerable<View> GetViewsReferencingTable(Table table)
        {
            Regex regexPotentialMatches = new Regex($@"(\[?[^\s]*\]?)?\.?\[?{table.Name}\]?", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            HashSet<View> matchingViews = new HashSet<View>();

            foreach (var view in Views)
            {
                MatchCollection matches = regexPotentialMatches.Matches(view.Definition);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var wholeMatch = match.Groups[0].Value;
                        if (wholeMatch.Contains("."))
                        {
                            var matchedSchema = match.Groups[1].Value.Replace("[", "").Replace("]", "").Trim();
                            if (matchedSchema.Equals(table.Schema, StringComparison.OrdinalIgnoreCase))
                            {
                                matchingViews.Add(view);
                            }
                        }
                        else
                        {
                            matchingViews.Add(view);
                        }
                    }
                }
            }

            return matchingViews;
        }

        /// <summary>
        /// Get a collection of <see cref="Routine"/> objects that MAY contain references
        /// to the provided table.
        /// </summary>
        /// <param name="table">The table for which to search.</param>
        /// <returns>A collection of distinct <see cref="Routine"/> objects.</returns>
        /// <remarks>This method uses a regular expression to find references; it may
        /// inadvertently match on comments or strings containing the table name.</remarks>
        public IEnumerable<Routine> GetRoutinesReferencingTable(Table table)
        {
            Regex regexPotentialMatches = new Regex($@"(\[?[^\s]*\]?)?\.?\[?{table.Name}\]?", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            HashSet<Routine> matchingRoutines = new HashSet<Routine>();

            foreach (var routine in Routines)
            {
                MatchCollection matches = regexPotentialMatches.Matches(routine.Definition);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var wholeMatch = match.Groups[0].Value;
                        if (wholeMatch.Contains("."))
                        {
                            var matchedSchema = match.Groups[1].Value.Replace("[", "").Replace("]", "").Trim();
                            if (matchedSchema.Equals(table.Schema, StringComparison.OrdinalIgnoreCase))
                            {
                                matchingRoutines.Add(routine);
                            }
                        }
                        else
                        {
                            matchingRoutines.Add(routine);
                        }
                    }
                }
            }

            return matchingRoutines;
        }

        /// <summary>
        /// Get the list of tables sorted by their dependencies.
        /// </summary>
        /// <returns>A collection of <see cref="Table"/> objects.</returns>
        public IEnumerable<Table> GetTablesSortedByDependency()
        {
            LinkedList<Table> tableList = new LinkedList<Table>(Tables);

            bool move = false;
            do
            {
                move = false;
                foreach (var table in Tables)
                {
                    int parentPosition = tableList.ToList().IndexOf(table);
                    var childrenFk = ForeignKeys.Where(f => f.ParentTable.FullName == table.FullName);

                    childrenFk.ToList().ForEach(c =>
                    {
                        if (c.ChildTable.FullName != table.FullName)
                        {
                            var childTableNode = tableList.Find(tableList.FirstOrDefault(t => t.FullName == c.ChildTable.FullName)!)!;
                            int childPosition = tableList.ToList().IndexOf(childTableNode.Value);
                            if (childPosition < parentPosition)
                            {
                                var parentTableNode = tableList.Find(table)!;
                                tableList.Remove(childTableNode);
                                tableList.AddAfter(parentTableNode, childTableNode);
                                move = true;
                            }
                        }
                    });
                }
            }
            while (move == true);

            return tableList;
        }
    }
}
