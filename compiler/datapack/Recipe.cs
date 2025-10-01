namespace Deco.Compiler.Datapack;

/// <summary>
/// Represents a Minecraft datapack recipe.
/// </summary>
public class Recipe(string json)
{
    /// <summary>
    /// The JSON content of the recipe.
    /// </summary>
    public string Json { get; } = json;
}
