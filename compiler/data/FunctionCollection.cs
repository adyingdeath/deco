using System.Collections.Generic;
using System.Linq;

namespace Deco.Compiler.Data
{
    /// <summary>
    /// A collection of all function-related resources and metadata.
    /// </summary>
    public class FunctionCollection
    {
        public List<McFunction> Items { get; } = new List<McFunction>();
        public McFunction OnLoadFunction { get; }
        public Dictionary<string, FunctionSignature> Table { get; } = new Dictionary<string, FunctionSignature>();
        public Dictionary<string, ResourceLocation> Locations { get; } = new Dictionary<string, ResourceLocation>();
        public Dictionary<string, DecoParser.FunctionContext> Contexts { get; } = new Dictionary<string, DecoParser.FunctionContext>();
        public int ParameterIdCounter { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCollection"/> class.
        /// </summary>
        /// <param name="onLoadFunction">The function to be executed on datapack load.</param>
        public FunctionCollection(McFunction onLoadFunction)
        {
            OnLoadFunction = onLoadFunction;
            Items.Add(OnLoadFunction);
        }

        public McFunction FindOrCreate(ResourceLocation location)
        {
            var existing = Items.FirstOrDefault(f => f.Location.ToString() == location.ToString());
            if (existing != null) return existing;

            var newFunc = new McFunction(location);
            Items.Add(newFunc);
            return newFunc;
        }
    }
}
