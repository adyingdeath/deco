using System.Collections.Generic;

namespace Deco.Compiler.Data {
    /// <summary>
    /// Holds the signature of a declared function (return type and parameters).
    /// </summary>
    public class FunctionSignature {
        public string ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();
    }
}
