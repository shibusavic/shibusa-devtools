using System;
using System.Collections.Generic;

namespace Shibusa.DevTools.Infrastructure.Schemas
{
    /// <summary>
    /// Reprsents a Sql Server View.
    /// </summary>
    public sealed class View : DbBase, IEquatable<View>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="schema">The view's schema.</param>
        /// <param name="name">The name of the view.</param>
        /// <param name="definition">The view's definition.</param>
        public View(string schema, string name, string definition) : base(schema, name)
        {
            Definition = string.IsNullOrWhiteSpace(definition) ? throw new ArgumentNullException(nameof(definition)) : definition;
        }

        /// <summary>
        /// Gets the view's defintion (the content).
        /// </summary>
        public string Definition { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as View);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(View? other)
        {
            return other != null &&
                   base.Equals(other) &&
                   Definition == other.Definition;
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Definition);
        }
    }
}
