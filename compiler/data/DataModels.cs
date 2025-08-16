using System;
using System.Collections.Generic;
using System.Linq;

namespace Deco.Compiler.Data {
    /// <summary>
    /// Represents a Minecraft resource location, like 'minecraft:stone' or 'deco:my_function'.
    /// </summary>
    public class ResourceLocation {
        public string Namespace { set; get; }
        public string Path { set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLocation"/> class from a string.
        /// </summary>
        /// <param name="location">The string representation of the resource location (e.g., 'minecraft:stone' or 'stone').</param>
        public ResourceLocation(string location) {
            SetLocation(location);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLocation"/> class with a specified path and optional namespace.
        /// </summary>
        /// <param name="path">The path part of the resource location.</param>
        /// <param name="namespace">The namespace part of the resource location. Defaults to "minecraft".</param>
        /// <exception cref="ArgumentException">Thrown if the path is null or empty.</exception>
        public ResourceLocation(string path, string @namespace = "minecraft") {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            Namespace = @namespace;
            Path = path;
        }

        /// <summary>
        /// Sets the resource location from a string.
        /// </summary>
        /// <param name="location">The string representation of the resource location (e.g., 'minecraft:stone' or 'stone').</param>
        public void SetLocation(string location) {
            if (string.IsNullOrWhiteSpace(location)) {
                throw new ArgumentException("Location string cannot be null or empty.", nameof(location));
            }

            if (location.Contains(':')) {
                var parts = location.Split(':', 2);
                // Basic validation: ensure parts are not empty after split for robustness
                if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1])) {
                    throw new FormatException($"Invalid resource location format: '{location}'. Expected 'namespace:path' or 'path'.");
                }
                this.Namespace = parts[0];
                this.Path = parts[1];
            }
            else {
                this.Namespace = "minecraft";
                this.Path = location;
            }
        }

        public override string ToString() => $"{Namespace}:{Path}";
    }

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

    

    /// <summary>
    /// Represents a single Minecraft function.
    /// </summary>
    public class McFunction(ResourceLocation location) {
        public ResourceLocation Location { get; } = location;
        public List<string> Commands { get; } = new List<string>();


        /// <summary>
        /// Prepend commands to command list.
        /// </summary>
        /// <param name="commands">Commands to prepend.</param>
        public void PrependCommands(string[] commands) {
            for (int i = commands.Length - 1; i >= 0; i--) {
                Commands.Insert(0, commands[i]);
            }
        }

        /// <summary>
        /// append commands to command list.
        /// </summary>
        /// <param name="commands">Commands to append.</param>
        public void AppendCommands(string[] commands) {
            Commands.AddRange(commands);
        }
    }

    /// <summary>
    /// Holds information about a function's parameter.
    /// </summary>
    public class ParameterInfo {
        public string Type { get; set; }
        public string Name { get; set; }
        public string StorageName { get; set; }
    }

    /// <summary>
    /// Holds the signature of a declared function (return type and parameters).
    /// </summary>
    public class FunctionSignature {
        public string ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();
    }

    /// <summary>
    /// A collection of all function-related resources and metadata.
    /// </summary>
    public class FunctionCollection {
        public List<McFunction> Items { get; } = [];
        public McFunction OnLoadFunction { get; }
        public Dictionary<string, FunctionSignature> Table { get; } = new();
        public Dictionary<string, ResourceLocation> Locations { get; } = new();
        public Dictionary<string, DecoParser.FunctionContext> Contexts { get; } = new();
        public int ParameterIdCounter { get; set; } = 0;

        public FunctionCollection(string mainNamespace) {
            // Create the default load function
            OnLoadFunction = new McFunction(new ResourceLocation(Util.GenerateRandomString(8), mainNamespace));
            Items.Add(OnLoadFunction);
        }

        public McFunction FindOrCreate(ResourceLocation location) {
            var existing = Items.FirstOrDefault(f => f.Location.ToString() == location.ToString());
            if (existing != null) return existing;

            var newFunc = new McFunction(location);
            Items.Add(newFunc);
            return newFunc;
        }
    }

    /// <summary>
    /// Represents the entire Minecraft data pack to be generated.
    /// </summary>
    public class DataPack {
        public string ID { get; }
        public string Name { get; }
        public string MainNamespace { get; }
        public FunctionCollection Functions { get; }
        public List<Tag> Tags { get; } = [];
        public Dictionary<string, string> Flags { get; } = [];

        public DataPack(string id, string name, string mainNamespace) {
            ID = id;
            Name = name;
            MainNamespace = mainNamespace;
            Functions = new FunctionCollection(mainNamespace);

            // Create the load tag and link it to the OnLoadFunction from the function collection
            var loadTag = new Tag(new ResourceLocation("minecraft:load"), TagType.Function);
            loadTag.Values.Add(Functions.OnLoadFunction.Location);
            Tags.Add(loadTag);
        }

        public Tag FindOrCreateTag(ResourceLocation location, TagType type) {
            var existing = Tags.FirstOrDefault(t => t.Location.ToString() == location.ToString() && t.Type == type);
            if (existing != null) return existing;

            var newTag = new Tag(location, type);
            Tags.Add(newTag);
            return newTag;
        }
    }
}