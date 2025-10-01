namespace Deco.Compiler.Pack;

/// <summary>
/// Represents a Minecraft datapack function.
/// </summary>
public class Function(List<string> commands) : HasResourceLocationBase<Function> {
    /// <summary>
    /// The list of commands in this function.
    /// </summary>
    public List<string> Commands { get; } = commands;
}
