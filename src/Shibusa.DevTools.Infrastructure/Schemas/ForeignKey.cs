namespace Shibusa.DevTools.Infrastructure.Schemas
{
    /// <summary>
    /// Represents a Sql Server Foreign Key.
    /// </summary>
    public sealed class ForeignKey : DbBase, IEquatable<ForeignKey>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ForeignKey"/> class.
        /// </summary>
        /// <param name="schema">The foreign key's schema.</param>
        /// <param name="name">The name of the foreign key.</param>
        /// <param name="parentTable">The parent table.</param>
        /// <param name="parentColumnName">The column in the parent table.</param>
        /// <param name="childTable">The child table.</param>
        /// <param name="childColumnName">The column in the child table.</param>
        public ForeignKey(string schema,
            string name,
            Table? parentTable,
            string? parentColumnName,
            Table? childTable,
            string? childColumnName) : base(schema, name)
        {
            ParentTable = parentTable ?? throw new ArgumentNullException(nameof(parentTable));
            ChildTable = childTable ?? throw new ArgumentNullException(nameof(childTable));
            ParentColumnName = string.IsNullOrWhiteSpace(parentColumnName) ? throw new ArgumentNullException(nameof(parentColumnName)) : parentColumnName;
            ChildColumnName = string.IsNullOrWhiteSpace(childColumnName) ? throw new ArgumentNullException(nameof(childColumnName)) : childColumnName;
        }

        /// <summary>
        /// The parent table.
        /// </summary>
        public Table ParentTable { get; }

        /// <summary>
        /// Gets the name of the column in the parent table.
        /// </summary>
        public string ParentColumnName { get; }

        /// <summary>
        /// Gets the child table.
        /// </summary>
        public Table ChildTable { get; }

        /// <summary>
        /// Gets the name of the column in the child table.
        /// </summary>
        public string ChildColumnName { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as ForeignKey);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(ForeignKey? other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<Table>.Default.Equals(ParentTable, other.ParentTable) &&
                   EqualityComparer<Table>.Default.Equals(ChildTable, other.ChildTable) &&
                   ChildColumnName == other.ChildColumnName &&
                   ParentColumnName == other.ParentColumnName;
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ParentTable, ChildTable, ChildColumnName, ParentColumnName);
        }
    }
}
