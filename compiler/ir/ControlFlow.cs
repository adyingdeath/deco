namespace Deco.Compiler.IR;

public interface ConditionalInstruction {
    Condition Condition { get; }
}

/// <summary>
/// IR instruction for unconditional jump to a label.
/// </summary>
public class JumpInstruction(
    LabelInstruction target
) : IRInstruction {
    public LabelInstruction Target { get; } = target;
    public bool IsFallThrough { get; set; } = false;
    public override string ToString() => (IsFallThrough ? "<- " : "") + $"Jump Label({Target.Label})";
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitJumpInstruction(this);
    public JumpInstruction FallThrough() {
        IsFallThrough = true;
        return this;
    }
}

/// <summary>
/// IR instruction for conditional jump (jump if condition is true).
/// </summary>
public class JumpIfInstruction(
    Condition condition, LabelInstruction target
) : JumpInstruction(target), ConditionalInstruction {
    public Condition Condition { get; } = condition;
    public override string ToString() => (IsFallThrough ? "<- " : "") + $"Jump Label({Target.Label}) if {Condition}";
}

/// <summary>
/// IR instruction for conditional jump (jump if condition is false).
/// </summary>
public class JumpUnlessInstruction(
    Condition condition, LabelInstruction target
) : JumpInstruction(target), ConditionalInstruction {
    public Condition Condition { get; } = condition;
    public override string ToString() => (IsFallThrough ? "<- " : "") + $"Jump Label({Target.Label}) unless {Condition}";
}

/// <summary>
/// Similar to Jump, but is specially designed for function call.
/// </summary>
public class CallInstruction(
    LabelInstruction target
) : JumpInstruction(target) {
    public override string ToString() => $"Call Label({Target.Label})";
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitCallInstruction(this);
}

/// <summary>
/// IR instruction representing a label (target for jumps).
/// IsAnchor is true if you want it to be an anchor label, which is peered with
/// another link and will be removed later.
/// </summary>
public class LabelInstruction(string label, bool isAnchor = false) : IRInstruction {
    public string Label { get; } = label;
    public bool IsAnchor { get; } = isAnchor;
    public List<IRInstruction> Instructions { get; } = [];
    public override string ToString() => (IsAnchor ? "#" : "") + $"Label {Label}:";
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitLabelInstruction(this);
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
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitLinkInstruction(this);
}

/// <summary>
/// IR instruction for function return.
/// Return value -> ()
/// </summary>
public class ReturnInstruction(Operand? value = null) : IRInstruction {
    public Operand? Value { get; } = value;
    public override string ToString() => $"Return {Value}";
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitReturnInstruction(this);
}

public class ReturnIfInstruction(
    Condition condition, Operand? value = null
) : IRInstruction, ConditionalInstruction {
    public Condition Condition { get; } = condition;
    public Operand? Value { get; } = value;
    public override string ToString() => $"Return {Value} if {Condition}";
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitReturnIfInstruction(this);
}

/// <summary>
/// This is a special instruction for fall through case. It's actually just
/// `Return 1 if Scoreboard(0) == 1`.
/// When creating jump for if, you will find a problem that `return` within the
/// if block will only exit and then back to the block where the `if` stands.
/// But we need the `return` to return from the function where the `if` is in.
/// This is a fall through. I tackle this by returning an integer `1` from the
/// if block and store it in Scoreboard(0), which is unused. Then use this
/// special instruction to test if Scoreboard(0) == 1, where it will return again
/// like it's shot through. Also work for loop.
/// </summary>
public class FallThroughInstruction() : ReturnIfInstruction(
    new Condition(
        ConditionType.Equal, new ScoreboardOperand("0"), new ConstantOperand("1")
    ), new ConstantOperand("1")
) {
    public override string ToString() => $"FallThrough";
}
