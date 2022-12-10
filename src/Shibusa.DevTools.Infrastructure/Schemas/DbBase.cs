namespace Shibusa.DevTools.Infrastructure.Schemas
{
    /// <summary>
    /// Represents the base object for all database objects.
    /// </summary>
    public abstract class DbBase : IEquatable<DbBase?>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DbBase"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="name">The name of the object.</param>
        public DbBase(string schema, string name)
        {
            Schema = string.IsNullOrWhiteSpace(schema) ? throw new ArgumentNullException(nameof(schema)) : schema;
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
        }

        /// <summary>
        /// Gets the schema for this object.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the name of this object.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the full name of the database object.
        /// </summary>
        public string FullName => $"{Schema}.{Name}";

        /// <summary>
        /// Gets the full name of the database objects with square brackets.
        /// </summary>
        public string FullNameWithBrackets => $"[{Schema}].[{Name}]";

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as DbBase);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(DbBase? other)
        {
            return other is not null &&
                   Schema == other.Schema &&
                   Name == other.Name;
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Schema, Name);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => FullName;
    }
}
