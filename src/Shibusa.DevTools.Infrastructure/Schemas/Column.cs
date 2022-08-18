namespace Shibusa.DevTools.Infrastructure.Schemas
{
    /// <summary>
    /// Represents a Sql Server table column.
    /// </summary>
    public sealed class Column : DbBase, IEquatable<Column>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Column"/> class.
        /// </summary>
        /// <param name="schema">The schema of the table.</param>
        /// <param name="name">The name of the column.</param>
        /// <param name="ordinalPosition">The ordinal position of the column.</param>
        /// <param name="columnDefault">The default value of the column.</param>
        /// <param name="isNullable">An indicator of whether the column is nullable.</param>
        /// <param name="dataType">The data type of the column.</param>
        /// <param name="maxLength">The max length of the column.</param>
        /// <param name="numericPrecision">The numeric precision of the column.</param>
        public Column(string schema,
            string name,
            int ordinalPosition,
            string? columnDefault,
            bool isNullable,
            string dataType,
            int maxLength,
            byte numericPrecision) : base(schema, name)
        {
            OrdinalPosition = ordinalPosition;
            ColumnDefault = columnDefault;
            IsNullable = isNullable;
            DataType = dataType;
            MaxLength = maxLength;
            NumericPrecision = numericPrecision;
        }

        /// <summary>
        /// Gets the ordinal position of the column.
        /// </summary>
        public int OrdinalPosition { get; }

        /// <summary>
        /// Gets the default value of the column.
        /// </summary>
        public string? ColumnDefault { get; }

        /// <summary>
        /// Gets an indicator of whether the column is nullable.
        /// </summary>
        public bool IsNullable { get; }

        /// <summary>
        /// Gets the data type of the column.
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// Gets the max length of the column.
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// Gets the numeric precision of the column.
        /// </summary>
        public byte NumericPrecision { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Column);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Column? other)
        {
            return other != null &&
                   base.Equals(other) &&
                   OrdinalPosition == other.OrdinalPosition &&
                   ColumnDefault == other.ColumnDefault &&
                   IsNullable == other.IsNullable &&
                   DataType == other.DataType &&
                   MaxLength == other.MaxLength &&
                   NumericPrecision == other.NumericPrecision;
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), OrdinalPosition, ColumnDefault, IsNullable, DataType, MaxLength, NumericPrecision);
        }
    }
}