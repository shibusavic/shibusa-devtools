namespace Shibusa.DevTools.Infrastructure.Schemas
{
    /// <summary>
    /// Represents a Sql Server Table.
    /// </summary>
    public sealed class Table : DbBase, IEquatable<Table>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="schema">The table's schema.</param>
        /// <param name="name">The table's name.</param>
        /// <param name="columns">A collection of <see cref="Column"/>s in the table.</param>
        public Table(string schema, string name, IEnumerable<Column> columns) : base(schema, name)
        {
            Columns = new SortedDictionary<int, Column>();
            if (columns?.Any() ?? false)
            {
                int ordinalPosition = 0;
                foreach (var column in columns)
                {
                    Columns.Add(++ordinalPosition, column);
                }
            }
        }

        /// <summary>
        /// Gets a dictionary of <see cref="Column"/> objects. 
        /// The key is the ordinal position of the column, starting with 1.
        /// </summary>
        public IDictionary<int, Column> Columns { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>   
        public override bool Equals(object? obj)
        {
            return Equals(obj as Table);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Table? other)
        {
            return other != null &&
                   base.Equals(other) &&
                   Columns.SequenceEqual(other.Columns);
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Columns);
        }
    }
}
