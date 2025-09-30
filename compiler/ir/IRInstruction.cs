namespace Deco.Compiler.IR;

/// <summary>
/// Abstract base class for all IR instructions.
/// Each instruction represents a single operation that can be executed.
/// </summary>
public abstract class IRInstruction {
    public override abstract string ToString();
}

/// <summary>
/// IR instruction for executing a Minecraft command.
/// </summary>
public class CommandInstruction(string command) : IRInstruction {
    public string Command { get; } = command;

    public override string ToString() => $"Command {Command}";
}

