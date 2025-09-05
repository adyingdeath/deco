using System.Collections.Generic;
using System.Linq;

namespace Deco.Compiler.Data {
    /// <summary>
    /// A collection of all function-related resources and metadata.
    /// </summary>
    public class FunctionCollection {
        public List<McFunction> McFunctions { get; } = new List<McFunction>();
        public Dictionary<string, DecoFunction> DecoFunctions { get; } = new Dictionary<string, DecoFunction>();
        public McFunction OnLoadFunction { get; }
        public int ParameterIdCounter { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCollection"/> class.
        /// </summary>
        /// <param name="onLoadFunction">The function to be executed on datapack load.</param>
        public FunctionCollection(McFunction onLoadFunction) {
            OnLoadFunction = onLoadFunction;
            McFunctions.Add(OnLoadFunction);
        }

        public McFunction FindOrCreateMcFunction(ResourceLocation location) {
            var existing = McFunctions.FirstOrDefault(f => f.Location.ToString() == location.ToString());
            if (existing != null) return existing;

            var newFunc = new McFunction(location);
            McFunctions.Add(newFunc);
            return newFunc;
        }
    }
}
