namespace Deco.Compiler.IR;

/// <summary>
/// IR instruction for unconditional jump to a label.
/// </summary>
public class JumpInstruction(
    LabelInstruction target
) : IRInstruction {
    public LabelInstruction Target { get; } = target;
    public override string ToString() => $"Jump Label({Target.Label})";
}

/// <summary>
/// IR instruction for conditional jump (jump if condition is true).
/// </summary>
public class JumpIfInstruction(
    Condition condition, LabelInstruction target
) : JumpInstruction(target) {
    public Condition Condition { get; } = condition;
    public override string ToString() => $"Jump Label({Target.Label}) if {Condition}";
}

/// <summary>
/// IR instruction for conditional jump (jump if condition is false).
/// </summary>
public class JumpUnlessInstruction(
    Condition condition, LabelInstruction target
) : JumpInstruction(target) {
    public Condition Condition { get; } = condition;
    public override string ToString() => $"Jump Label({Target.Label}) unless {Condition}";
}

/// <summary>
/// Similar to Jump, but is specially designed for function call.
/// </summary>
public class CallInstruction(
    LabelInstruction target
) : JumpInstruction(target) {
    public override string ToString() => $"Call Label({Target.Label})";
}

/// <summary>
/// IR instruction representing a label (target for jumps).
/// </summary>
public class LabelInstruction(string label) : IRInstruction {
    public string Label { get; } = label;
    public override string ToString() => $"Label {Label}:";
}

/// <summary>
/// Represents a "linked label" instruction. This instruction acts as a placeholder
/// that the compiler will replace with a sequence of instructions at a later stage.
/// </summary>
/// <remarks>
/// A linked label is similar to a normal label but is specifically designed to be
/// associated with a <see cref="LinkInstruction"/>. When a <see cref="LinkInstruction"/>
/// targets a <see cref="LabelInstruction"/> (e.g., `Link Label(a)`), the compiler
/// replaces the <see cref="LinkInstruction"/> with the entire block of instructions
/// starting from the targeted <see cref="LabelInstruction"/> (`Label(a)`)
/// up to the next <see cref="LabelInstruction"/> that follows it.
/// This mechanism allows for dynamic code insertion or function-like expansion
/// at the instruction level.
/// </remarks>
public class LinkInstruction(
    LabelInstruction target
) : IRInstruction {
    /// <summary>
    /// Gets the <see cref="LabelInstruction"/> that this <see cref="LinkInstruction"/> targets.
    /// The compiler will replace this instruction with the sequence of instructions
    /// starting from the target label.
    /// </summary>
    public LabelInstruction Target { get; } = target;

    public override string ToString() => $"Link Label({Target.Label})";
}

/// <summary>
/// IR instruction for function return.
/// Return value -> ()
/// </summary>
public class ReturnInstruction(Operand? value = null) : IRInstruction {
    public Operand? Value { get; } = value;
    public override string ToString() => $"Return {Value}";
}

public class ReturnIfInstruction(Condition condition, Operand? value = null) : IRInstruction {
    public Condition Condition { get; } = condition;
    public Operand? Value { get; } = value;
    public override string ToString() => $"Return {Value} if {Condition}";
}
