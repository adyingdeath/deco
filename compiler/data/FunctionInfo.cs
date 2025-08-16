using System.Collections.Generic;

namespace Deco.Compiler.Data
{
    /// <summary>
    /// Holds information about a function's parameter.
    /// </summary>
    public class ParameterInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string StorageName { get; set; }
    }

    /// <summary>
    /// Holds the signature of a declared function (return type and parameters).
    /// </summary>
    public class FunctionSignature
    {
        public string ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();
    }
}
