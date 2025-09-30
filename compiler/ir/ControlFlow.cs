namespace Deco.Compiler.IR;

/// <summary>
/// IR instruction for unconditional jump to a label.
/// </summary>
public class JumpInstruction(
    LabelInstruction target
) : IRInstruction(IROpCode.Jump) {
    public LabelInstruction Target { get; } = target;
    public override string ToString() => $"Jump Label({Target.Label})";
}

/// <summary>
/// IR instruction for conditional jump (jump if condition is true).
/// </summary>
public class JumpIfInstruction(
    Condition condition, LabelInstruction target
) : IRInstruction(IROpCode.JumpIf) {
    public Condition Condition { get; } = condition;
    public LabelInstruction Target { get; } = target;
    public override string ToString() => $"Jump Label({Target.Label}) if {Condition}";
}

/// <summary>
/// IR instruction for conditional jump (jump if condition is false).
/// </summary>
public class JumpUnlessInstruction(
    Condition condition, LabelInstruction target
) : IRInstruction(IROpCode.JumpUnless) {
    public Condition Condition { get; } = condition;
    public LabelInstruction Target { get; } = target;
    public override string ToString() => $"Jump Label({Target.Label}) unless {Condition}";
}

/// <summary>
/// IR instruction representing a label (target for jumps).
/// </summary>
public class LabelInstruction(string label) : IRInstruction(IROpCode.Label) {
    public string Label { get; } = label;
    public override string ToString() => $"Label {Label}:";
}

/// <summary>
/// IR instruction for function return.
/// Return value -> ()
/// </summary>
public class ReturnInstruction(Operand? value = null) : IRInstruction(IROpCode.Return) {
    public Operand? Value { get; } = value;
    public override string ToString() => $"Return {Value}";
}

public class ReturnIfInstruction(Condition condition, Operand? value = null) : IRInstruction(IROpCode.ReturnIf) {
    public Condition Condition { get; } = condition;
    public Operand? Value { get; } = value;
    public override string ToString() => $"Return {Value} if {Condition}";
}
