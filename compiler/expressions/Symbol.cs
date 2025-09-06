using Deco.Compiler.Library.Types;

namespace Deco.Compiler.Expressions {
    public class Symbol {
        public string Name { get; }
        public IDecoType Type { get; }
        public string StorageName { get; }
        public bool IsInitialized { get; set; }

        public Symbol(string name, IDecoType type, string storageName) {
            Name = name;
            Type = type;
            StorageName = storageName;
            IsInitialized = false;
        }
    }
}
