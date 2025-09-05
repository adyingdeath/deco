using Deco.Compiler.Expressions;

namespace Deco.Compiler.Data {
    /// <summary>
    /// Holds information about a function's parameter.
    /// </summary>
    public class ParameterInfo : Symbol {
        public ParameterInfo(string name, string type, string storageName)
            : base(name, type, storageName) {
        }
    }
}
