namespace Deco.Compiler.Library.Types {
    /// <summary>
    /// A special IDecoType representing a compile-time constant of another type.
    /// This type cannot be used for variables, only for function parameters.
    /// </summary>
    public class ConstType<T> : IDecoType where T : IDecoType {
        public T BaseType { get; }

        public ConstType(T baseType) {
            BaseType = baseType;
        }

        public string Name => $"Const<{BaseType.Name}>";

        public void Assign(LibContext context, Variable target, Variable source) {
            // This should never be called.
            throw new System.NotSupportedException($"Cannot assign to or from a '{Name}' type. It is a compile-time only constraint.");
        }

        public void Initialize(LibContext context, Variable target) {
            // This should also never be called.
            throw new System.NotSupportedException($"Cannot create a variable of type '{Name}'. It is a compile-time only constraint.");
        }
    }
}
