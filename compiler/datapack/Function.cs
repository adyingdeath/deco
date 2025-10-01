namespace Deco.Compiler.Datapack;

/// <summary>
/// Represents a Minecraft datapack function.
/// </summary>
public class Function(List<string> commands)
{
    /// <summary>
    /// The list of commands in this function.
    /// </summary>
    public List<string> Commands { get; } = commands;
}
