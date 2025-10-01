namespace Deco.Compiler.Datapack;

/// <summary>
/// Represents a Minecraft datapack predicate.
/// </summary>
public class Predicate(string json)
{
    /// <summary>
    /// The JSON content of the predicate.
    /// </summary>
    public string Json { get; } = json;
}
