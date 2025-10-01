namespace Deco.Compiler.Datapack;

/// <summary>
/// Represents a Minecraft datapack loot table.
/// </summary>
public class LootTable(string json)
{
    /// <summary>
    /// The JSON content of the loot table.
    /// </summary>
    public string Json { get; } = json;
}
