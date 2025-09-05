using System.Collections.Generic;

namespace Deco.Compiler.Data {
    /// <summary>
    /// Defines the type of a tag, which determines its directory.
    /// </summary>
    public enum TagType {
        Function,
        Items,
        Blocks,
        EntityTypes,
        GameEvents,
        Biomes
    }

    /// <summary>
    /// Represents a generic tag, which is a collection of resource locations.
    /// </summary>
    public class Tag(ResourceLocation location, TagType type) {
        public ResourceLocation Location { get; } = location;
        public TagType Type { get; } = type;
        public List<ResourceLocation> Values { get; } = new List<ResourceLocation>();
    }
}
