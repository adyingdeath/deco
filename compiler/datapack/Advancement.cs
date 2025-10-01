namespace Deco.Compiler.Datapack;

/// <summary>
/// Represents a Minecraft datapack advancement.
/// </summary>
public class Advancement(string json)
{
    /// <summary>
    /// The JSON content of the advancement.
    /// </summary>
    public string Json { get; } = json;
}
