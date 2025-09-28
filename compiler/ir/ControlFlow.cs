namespace Deco.Compiler.IR;

/// <summary>
/// IR instruction for unconditional jump to a label.
/// </summary>
public class JumpInstruction(
    LabelInstruction target
) : IRInstruction(IROpCode.Jump) {
    public LabelInstruction Target { get; } = target;

    public override List<object> Operands => [Target];
}

/// <summary>
/// IR instruction for conditional jump (jump if condition is true).
/// </summary>
public class JumpIfInstruction(
    Condition condition, LabelInstruction target
) : IRInstruction(IROpCode.JumpIf) {
    public Condition Condition { get; } = condition;
    public LabelInstruction Target { get; } = target;

    public override List<object> Operands => [Condition, Target];
}

/// <summary>
/// IR instruction for conditional jump (jump if condition is false).
/// </summary>
public class JumpUnlessInstruction(
    Condition condition, LabelInstruction target
) : IRInstruction(IROpCode.JumpUnless) {
    public Condition Condition { get; } = condition;
    public LabelInstruction Target { get; } = target;

    public override List<object> Operands => [Condition, Target];
}

/// <summary>
/// IR instruction representing a label (target for jumps).
/// </summary>
public class LabelInstruction(string label) : IRInstruction(IROpCode.Label) {
    public string Label { get; } = label;

    public override List<object> Operands => [Label];
}

/// <summary>
/// IR instruction for function return.
/// Return value -> ()
/// </summary>
public class ReturnInstruction(object? value = null) : IRInstruction(IROpCode.Return) {
    public object? Value { get; } = value;

    public override List<object> Operands => Value != null ? [Value] : [];
}

/// <summary>
/// IR instruction for no operation.
/// </summary>
public class NopInstruction() : IRInstruction(IROpCode.Nop) {
    // No operands
}
