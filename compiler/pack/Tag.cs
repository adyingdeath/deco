namespace Deco.Compiler.Pack;

public enum TagType {
    Block,
    Item,
    Function,
    Fluid,
    EntityType,
    GameEvent,
    Biome,
    FlatLevelGeneratorPreset,
    WorldPreset,
    Structure,
    CatVariant,
    PointOfInterestType,
    PaintingVariant,
    BannerPattern,
    Instrument,
    DamageType,
    Enchantment,
    Dialog
}

/// <summary>
/// Represents a Minecraft datapack tag.
/// </summary>
public class Tag(TagType type, List<string> entries) : HasResourceLocationBase<Tag> {
    public TagType Type { get; } = type;
    public bool Replace { get; set; } = false;
    /// <summary>
    /// The list of entries in this tag.
    /// </summary>
    public List<string> Entries { get; } = entries;
}
