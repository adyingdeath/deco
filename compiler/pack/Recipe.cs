namespace Deco.Compiler.Pack;

/// <summary>
/// Represents a Minecraft datapack recipe.
/// </summary>
public class Recipe(string json) : HasResourceLocationBase<Recipe> {
    /// <summary>
    /// The JSON content of the recipe.
    /// </summary>
    public string Json { get; } = json;
}
