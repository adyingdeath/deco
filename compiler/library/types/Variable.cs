namespace Deco.Compiler.Library.Types {
    /// <summary>
    /// Represents an actual variable instance in the compiled code.
    /// For example, a Vector3 might be stored under a unique storage name.
    /// </summary>
    public class Variable {
        public IDecoType Type { get; }
        public string StorageName { get; }

        public Variable(IDecoType type, string storageName) {
            Type = type;
            StorageName = storageName;
        }
    }
}
