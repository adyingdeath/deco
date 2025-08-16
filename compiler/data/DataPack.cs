using System.Collections.Generic;
using System.Linq;

namespace Deco.Compiler.Data
{
    /// <summary>
    /// Represents the entire Minecraft data pack to be generated.
    /// </summary>
    public class DataPack
    {
        public string ID { get; }
        public string Name { get; }
        public string MainNamespace { get; }
        public FunctionCollection Functions { get; }
        public List<Tag> Tags { get; } = new List<Tag>();
        public Dictionary<string, string> Flags { get; } = new Dictionary<string, string>();

        public DataPack(string id, string name, string mainNamespace)
        {
            ID = id;
            Name = name;
            MainNamespace = mainNamespace;

            // Create the special OnLoad function and initialize the function collection with it
            var onLoadFunction = new McFunction(new ResourceLocation(Util.GenerateRandomString(8), mainNamespace));
            Functions = new FunctionCollection(onLoadFunction);

            // Create the minecraft:load tag and link it to the OnLoadFunction
            var loadTag = new Tag(new ResourceLocation("minecraft:load"), TagType.Function);
            loadTag.Values.Add(Functions.OnLoadFunction.Location);
            Tags.Add(loadTag);
        }

        public Tag FindOrCreateTag(ResourceLocation location, TagType type)
        {
            var existing = Tags.FirstOrDefault(t => t.Location.ToString() == location.ToString() && t.Type == type);
            if (existing != null) return existing;

            var newTag = new Tag(location, type);
            Tags.Add(newTag);
            return newTag;
        }
    }
}
