namespace Deco.Compiler.Pack;

/// <summary>
/// Represents a Minecraft datapack advancement.
/// </summary>
public class Advancement(string json) : HasResourceLocationBase<Advancement> {
    /// <summary>
    /// The JSON content of the advancement.
    /// </summary>
    public string Json { get; } = json;
}
