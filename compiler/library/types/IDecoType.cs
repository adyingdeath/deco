namespace Deco.Compiler.Library.Types {
    /// <summary>
    /// Represents a type within the Deco language.
    /// Every type, built-in or user-defined, must implement this.
    /// </summary>
    public interface IDecoType {
        /// <summary>
        /// Which packages is this type defined in. Used to compare if two types are the same
        /// </summary>
        string Package { get; }
        /// <summary>
        /// The name of the type as used in Deco code (e.g., "int", "Player").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Compare if two types are the same.
        /// </summary>
        public bool Equals(IDecoType type) {
            return Package == type.Package && Name == type.Name;
        }

        /// <summary>
        /// Generates the commands to assign one variable of this type to another.
        /// The compiler will call this for statements like `var_a = var_b;`
        /// </summary>
        /// <param name="context">Provides access to the compiler's state, like the current McFunction.</param>
        /// <param name="target">The variable instance being assigned to.</param>
        /// <param name="source">The variable instance being assigned from.</param>
        void Assign(LibContext context, Variable target, Variable source);

        /// <summary>
        /// Generates the commands to initialize a new variable of this type to its default value.
        /// </summary>
        /// <param name="context">Provides access to the compiler's state.</param>
        /// <param name="target">The variable instance to initialize.</param>
        void Initialize(LibContext context, Variable target);
    }
}
