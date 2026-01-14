namespace Deco.Compiler.IR;

/// <summary>
/// Abstract base class for all IR instructions.
/// Each instruction represents a single operation that can be executed.
/// </summary>
public abstract class IRInstruction {
    public override abstract string ToString();

    public abstract T Accept<T>(IRVisitor<T> visitor);
}

/// <summary>
/// Represents a complete IR Program consisting of multiple functions.
/// </summary>
public class IrProgram {
    public string DataPackId { get; set; } = "";
    public List<IrFunction> Functions { get; set; } = [];
}

/// <summary>
/// Represents a discrete function (corresponds to a .mcfunction file).
/// It has a name (path) and a sequence of instructions.
/// </summary>
public class IrFunction(string name, List<IRInstruction> instructions) {
    public string Name { get; } = name;
    public List<IRInstruction> Instructions { get; } = instructions;

    public override string ToString() {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Function {Name}:");
        foreach (var inst in Instructions) {
            sb.AppendLine($"  {inst}");
        }
        return sb.ToString();
    }
}

/// <summary>
/// IR instruction for executing a Minecraft command.
/// </summary>
public class CommandInstruction(string command) : IRInstruction {
    public string Command { get; } = command;

    public override string ToString() => $"Command {Command}";

    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitCommandInstruction(this);
}

public class PushInstruction(VariableOperand operand) : IRInstruction {
    public VariableOperand Operand = operand;

    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitPushInstruction(this);

    public override string ToString() => $"Push {Operand} => Stack({Operand.StackName})";
}

public class PopInstruction(VariableOperand operand) : IRInstruction {
    public VariableOperand Operand = operand;

    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitPopInstruction(this);

    public override string ToString() => $"Pop Stack({Operand.StackName}) => {Operand}";
}