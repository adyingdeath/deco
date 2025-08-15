
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deco.Compiler.Data
{
    /// <summary>
    /// Represents a Minecraft resource location, like 'minecraft:stone' or 'deco:my_function'.
    /// </summary>
    public class ResourceLocation
    {
        public string Namespace { get; }
        public string Path { get; }

        public ResourceLocation(string path, string @namespace = "deco")
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            
            Namespace = @namespace;
            Path = path;
        }

        public override string ToString() => $"{Namespace}:{Path}";
    }

    /// <summary>
    /// Defines the type of a tag, which determines its directory.
    /// </summary>
    public enum TagType
    {
        Functions,
        Items,
        Blocks,
        EntityTypes,
        GameEvents,
        Biomes
    }

    /// <summary>
    /// Represents a generic tag, which is a collection of resource locations.
    /// </summary>
    public class Tag
    {
        public ResourceLocation Location { get; }
        public TagType Type { get; }
        public List<ResourceLocation> Values { get; } = new List<ResourceLocation>();

        public Tag(ResourceLocation location, TagType type)
        {
            Location = location;
            Type = type;
        }
    }

    /// <summary>
    /// Represents a single Minecraft function.
    /// </summary>
    public class McFunction
    {
        public ResourceLocation Location { get; }
        public List<string> Commands { get; } = new List<string>();

        public McFunction(ResourceLocation location)
        {
            Location = location;
        }
    }

    /// <summary>
    /// Represents the entire Minecraft data pack to be generated.
    /// </summary>
    public class DataPack
    {
        public string Name { get; }
        public List<McFunction> Functions { get; } = new List<McFunction>();
        public List<Tag> Tags { get; } = new List<Tag>();

        public DataPack(string name = "generated_datapack")
        {
            Name = name;
        }

        public McFunction FindOrCreateFunction(ResourceLocation location)
        {
            var existing = Functions.FirstOrDefault(f => f.Location.ToString() == location.ToString());
            if (existing != null) return existing;

            var newFunc = new McFunction(location);
            Functions.Add(newFunc);
            return newFunc;
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
