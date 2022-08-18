namespace Shibusa.DevTools.Infrastructure.Schemas
{
    /// <summary>
    /// Represents a DTO for foreign keys.
    /// </summary>
    internal struct ForeignKeyDto : IEquatable<ForeignKeyDto>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ForeignKeyDto"/> class.
        /// </summary>
        /// <param name="name">The name of the foreign key.</param>
        /// <param name="schema">The schema of the child table.</param>
        /// <param name="tableName">The name of the child table name.</param>
        /// <param name="columnName">The name of the child table's column.</param>
        /// <param name="referenceSchema">The schema of the parent table.</param>
        /// <param name="referenceTableName">The name of the parent table name.</param>
        /// <param name="referenceColumnName">The name of the parent table's column name.</param>
        public ForeignKeyDto(string name,
            string schema,
            string tableName,
            string columnName,
            string referenceSchema,
            string referenceTableName,
            string referenceColumnName)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
            Schema = string.IsNullOrWhiteSpace(schema) ? throw new ArgumentNullException(nameof(schema)) : schema;
            TableName = string.IsNullOrWhiteSpace(tableName) ? throw new ArgumentNullException(nameof(tableName)) : tableName;
            ColumnName = string.IsNullOrWhiteSpace(columnName) ? throw new ArgumentNullException(nameof(columnName)) : columnName;
            ReferenceSchema = string.IsNullOrWhiteSpace(referenceSchema) ? throw new ArgumentNullException(nameof(referenceSchema)) : referenceSchema;
            ReferenceTableName = string.IsNullOrWhiteSpace(referenceTableName) ? throw new ArgumentNullException(nameof(referenceTableName)) : referenceTableName;
            ReferenceColumnName = string.IsNullOrWhiteSpace(referenceColumnName) ? throw new ArgumentNullException(nameof(referenceColumnName)) : referenceColumnName;
        }

        /// <summary>
        /// Gets the schema of the child table.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the name of the foreign key.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the schema of the parent table.
        /// </summary>
        public string ReferenceSchema { get; }

        /// <summary>
        /// Gets the name of the parent table.
        /// </summary>
        public string ReferenceTableName { get; }

        /// <summary>
        /// Gets the name of the column in the parent table.
        /// </summary>
        public string ReferenceColumnName { get; }

        /// <summary>
        /// Gets the name of the child table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Gets the name of the child table's column.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is ForeignKeyDto dto && Equals(dto);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(ForeignKeyDto other)
        {
            return Schema == other.Schema &&
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
    }
}
