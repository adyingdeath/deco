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
/// IR instruction for executing a Minecraft command.
/// </summary>
public class CommandInstruction(string command) : IRInstruction {
    public string Command { get; } = command;

    public override string ToString() => $"Command {Command}";

    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitCommandInstruction(this);
}

public class ProgramInstruction : IRInstruction {
    public List<LabelInstruction> Labels { get; } = [];
    public override string ToString() => "Program:";

    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitProgram(this);
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