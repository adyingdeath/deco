namespace Deco.Compiler.Pack;

/// <summary>
/// Represents a Minecraft datapack predicate.
/// </summary>
public class Predicate(string json) : HasResourceLocationBase<Predicate> {
    /// <summary>
    /// The JSON content of the predicate.
    /// </summary>
    public string Json { get; } = json;
}
