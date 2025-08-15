
using System.Collections.Generic;

namespace Deco.Compiler.Data
{
    /// <summary>
    /// Represents a single Minecraft function, containing its name and commands.
    /// </summary>
    public class McFunction
    {
        public string Namespace { get; }
        public string Name { get; }
        public List<string> Commands { get; } = new List<string>();

        public McFunction(string name, string @namespace = "deco")
        {
            Name = name;
            Namespace = @namespace;
        }
    }

    /// <summary>
    /// Represents the entire Minecraft data pack to be generated.
    /// It holds all the functions.
    /// </summary>
    public class DataPack
    {
        public string Name { get; }
        public List<McFunction> Functions { get; } = new List<McFunction>();

        public DataPack(string name = "generated_datapack")
        {
            Name = name;
        }

        public McFunction AddFunction(string name, string @namespace = "deco")
        {
            var function = new McFunction(name, @namespace);
            Functions.Add(function);
            return function;
        }
    }
}
