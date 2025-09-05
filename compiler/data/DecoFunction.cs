using Deco.Compiler.Expressions;

namespace Deco.Compiler.Data {
    /// <summary>
    /// Represents a complete function declared in the source code,
    /// linking its definition, signature, and generated McFunction.
    /// </summary>
    public class DecoFunction {
        public string Name { get; }
        public FunctionSignature Signature { get; }
        public McFunction McFunction { get; }
        public DecoParser.FunctionContext Context { get; }
        public SymbolTable SymbolTable { get; }

        public DecoFunction(string name, FunctionSignature signature, McFunction mcFunction, DecoParser.FunctionContext context, SymbolTable symbolTable) {
            Name = name;
            Signature = signature;
            McFunction = mcFunction;
            Context = context;
            SymbolTable = symbolTable;
        }
    }
}
