using System.Text.Json;

namespace Deco.Compiler.Pack;

public class PackMcmeta(int packFormat, string description) {
    public int PackFormat { get; set; } = packFormat;
    public string Description { get; set; } = description;
    public override string ToString() => JsonSerializer.Serialize(this);
}

/// <summary>
/// Represents a Minecraft datapack's resources.
/// </summary>
public class Datapack {
    /// <summary>
    /// The ID of this datapack, used for scoreboard objectives and storage namespacing.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The namespace for this datapack.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// The pack.mcmeta JSON content.
    /// </summary>
    public PackMcmeta PackMcmeta { get; set; } = new(15, "test");

    /// <summary>
    /// Function resource locations to Function objects mapping.
    /// </summary>
    public List<Function> Functions { get; } = [];

    /// <summary>
    /// Advancement resource locations to Advancement objects mapping.
    /// </summary>
    public List<Advancement> Advancements { get; } = [];

    /// <summary>
    /// Loot table resource locations to LootTable objects mapping.
    /// </summary>
    public List<LootTable> LootTables { get; } = [];

    /// <summary>
    /// Tag resource locations to Tag objects mapping (for function tags, item tags, etc.).
    /// </summary>
    public List<Tag> Tags { get; } = [];

    /// <summary>
    /// Predicate resource locations to Predicate objects mapping.
    /// </summary>
    public List<Predicate> Predicates { get; } = [];

    /// <summary>
    /// Recipe resource locations to Recipe objects mapping.
    /// </summary>
    public List<Recipe> Recipes { get; } = [];

    public Datapack(string id, string namespaceName) {
        Id = id;
        Namespace = namespaceName;
        // We need these two tags by default. Register load tag for main function.
        Tags.Add(
            new Tag(TagType.Function, [])
            .SetLocation(new ResourceLocation("minecraft", "load"))
        );
        Tags.Add(
            new Tag(TagType.Function, [])
            .SetLocation(new ResourceLocation("minecraft", "tick"))
        );
    }
}
