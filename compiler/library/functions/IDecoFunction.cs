using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using Deco.Compiler.Library.Types;

namespace Deco.Compiler.Library.Functions {
    public interface IDecoFunction {
        /// <summary>
        /// The name of the function as used in Deco code.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The function's signature, used by the compiler for type checking.
        /// </summary>
        FunctionSignature Signature { get; }

        /// <summary>
        /// Generates the commands for a call to this function.
        /// </summary>
        /// <param name="context">Compiler context.</param>
        /// <param name="arguments">A list of evaluated operands passed to the function.</param>
        /// <returns>An operand representing the return value. Can be a void/dummy operand.</returns>
        Operand Execute(LibContext context, List<Operand> arguments);
    }

    /// <summary>
    /// Extended function signature for the new library system
    /// </summary>
    public class ExtendedFunctionSignature {
        public IDecoType ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();
        public bool IsVariadic { get; set; }

        // Helper method to convert to string-based signature for compatibility
        public FunctionSignature ToLegacySignature() {
            var sig = new FunctionSignature();
            sig.ReturnType = ReturnType.Name;
            sig.Parameters.AddRange(Parameters);
            return sig;
        }
    }
}
