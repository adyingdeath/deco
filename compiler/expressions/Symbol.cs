namespace Deco.Compiler.Expressions
{
    public class Symbol
    {
        public string Name { get; }
        public string Type { get; }
        public string StorageName { get; }
        public bool IsInitialized { get; set; }

        public Symbol(string name, string type, string storageName)
        {
            Name = name;
            Type = type;
            StorageName = storageName;
            IsInitialized = false;
        }
    }
}
