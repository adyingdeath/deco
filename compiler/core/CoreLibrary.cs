using Deco.Compiler.Library.Types;
using Deco.Compiler.Library;

namespace Deco.Compiler.Core {
    /// <summary>
    /// Core library containing all built-in types and functions
    /// </summary>
    public class CoreLibrary : IDecoLibrary {
        public void Register(LibraryRegistry registry) {
            // Register built-in types
            RegisterTypes(registry);

            // Register built-in functions
            RegisterFunctions(registry);
        }

        private void RegisterTypes(LibraryRegistry registry) {
            registry.AddType(new VoidType());
            registry.AddType(new IntType());
            registry.AddType(new BoolType());
            registry.AddType(new StringType());
            registry.AddType(new FloatType());
        }

        private void RegisterFunctions(LibraryRegistry registry) {
            registry.AddFunction(new Functions.PrintFunction());
            registry.AddFunction(new Functions.FunctionFunction());
        }
    }
}
