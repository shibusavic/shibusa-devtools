namespace Shibusa.DevTools.Infrastructure.Schemas
{
    /// <summary>
    /// Represents a Sql Server Routine.
    /// </summary>
    public sealed class Routine : DbBase, IEquatable<Routine>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Routine"/> class.
        /// </summary>
        /// <param name="schema">The routine's schema.</param>
        /// <param name="name">The routine's name.</param>
        /// <param name="definition">The routine's definition.</param>
        /// <param name="routineType">The type of routine.</param>
        public Routine(string schema, string name, string definition, string routineType) : base(schema, name)
        {
            Definition = string.IsNullOrWhiteSpace(definition) ? throw new ArgumentNullException(nameof(definition)) : definition;
            RoutineType = string.IsNullOrWhiteSpace(routineType) ? throw new ArgumentNullException(nameof(routineType)) : routineType;
        }

        /// <summary>
        /// Gets the routine type.
        /// </summary>
        public string RoutineType { get; }

        /// <summary>
        /// Gets the routine's definition (its content).
        /// </summary>
        public string Definition { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Routine);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Routine? other)
        {
            return other != null &&
                   base.Equals(other) &&
                   RoutineType == other.RoutineType &&
                   Definition == other.Definition;
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), RoutineType, Definition);
        }
    }
}
