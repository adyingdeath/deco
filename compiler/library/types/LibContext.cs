using Deco.Compiler.Data;
using Deco.Compiler.Expressions;

namespace Deco.Compiler.Library.Types {
    /// <summary>
    /// A context object passed to library functions, giving them controlled
    /// access to the compiler to add commands, manage temporary variables, etc.
    /// </summary>
    public class LibContext {
        public LibContext(McFunction currentMcFunction, DataPack dataPack, SymbolTable symbolTable) {
            CurrentMcFunction = currentMcFunction;
            DataPack = dataPack;
            SymbolTable = symbolTable;
        }

        public McFunction CurrentMcFunction { get; }
        public DataPack DataPack { get; }
        public SymbolTable SymbolTable { get; }

        /// <summary>
        /// Gets the next temporary variable name for storage/scoreboard operations
        /// </summary>
        public string GetNextTemp() {
            // This would ideally get a unique temp name from the compiler
            // For now, we'll generate a simple unique name
            return $"temp_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}
