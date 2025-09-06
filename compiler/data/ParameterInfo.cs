using Deco.Compiler.Expressions;
using Deco.Compiler.Library.Types;

namespace Deco.Compiler.Data {
    /// <summary>
    /// Holds information about a function's parameter.
    /// </summary>
    public class ParameterInfo : Symbol {
        public ParameterInfo(string name, IDecoType type, string storageName)
            : base(name, type, storageName) {
        }
    }
}
