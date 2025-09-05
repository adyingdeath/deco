namespace Deco.Compiler.Data {
    /// <summary>
    /// Holds the signature of a declared function (return type and parameters).
    /// Now uses the new library type system with IDecoType.
    /// </summary>
    public class FunctionSignature {
        public IDecoType ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();
    }
}
