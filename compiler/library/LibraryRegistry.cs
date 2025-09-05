using Deco.Compiler.Library.Types;
using Deco.Compiler.Library.Functions;

namespace Deco.Compiler.Library {
    public class LibraryRegistry {
        private readonly Dictionary<string, IDecoType> _types = new Dictionary<string, IDecoType>();
        private readonly Dictionary<string, IDecoFunction> _functions = new Dictionary<string, IDecoFunction>();

        public void AddType(IDecoType type) {
            if (_types.ContainsKey(type.Name)) {
                throw new System.ArgumentException($"A type with the name '{type.Name}' is already registered.");
            }
            _types[type.Name] = type;
        }

        public void AddFunction(IDecoFunction function) {
            if (_functions.ContainsKey(function.Name)) {
                throw new System.ArgumentException($"A function with the name '{function.Name}' is already registered.");
            }
            _functions[function.Name] = function;
        }

        // Internal methods for compiler to access the registry
        internal IDecoType GetType(string name) => _types.GetValueOrDefault(name);
        internal IDecoFunction GetFunction(string name) => _functions.GetValueOrDefault(name);

        // Public getters for external access
        public IEnumerable<IDecoType> GetAllTypes() => _types.Values;
        public IEnumerable<IDecoFunction> GetAllFunctions() => _functions.Values;
    }
}
