namespace Deco.Compiler.Pack;

/// <summary>
/// Represents a Minecraft datapack loot table.
/// </summary>
public class LootTable(string json) : HasResourceLocationBase<LootTable> {
    /// <summary>
    /// The JSON content of the loot table.
    /// </summary>
    public string Json { get; } = json;
}
