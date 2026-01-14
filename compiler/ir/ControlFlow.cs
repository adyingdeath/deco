namespace Deco.Compiler.IR;

/// <summary>
/// IR instruction for function call.
/// Maps to: execute run function [target]
/// </summary>
public class CallInstruction(string targetFunction) : IRInstruction {
    public string TargetFunction { get; } = targetFunction;
    public override string ToString() => $"Call {TargetFunction}";
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitCallInstruction(this);
}

/// <summary>
/// IR instruction for conditional function call.
/// Maps to: execute if/unless [condition] run function [target]
/// </summary>
public class CallIfInstruction(
    Condition condition, string targetFunction, bool isUnless = false
) : IRInstruction {
    public Condition Condition { get; } = condition;
    public string TargetFunction { get; } = targetFunction;
    public bool IsUnless { get; } = isUnless;

    public override string ToString() => 
        $"Call {TargetFunction} {(IsUnless ? "unless" : "if")} {Condition}";
    
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitCallIfInstruction(this);
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

/// <summary>
/// Conditional return.
/// </summary>
public class ReturnIfInstruction(
    Condition condition, Operand? value = null
) : IRInstruction {
    public Condition Condition { get; } = condition;
    public Operand? Value { get; } = value;
    public override string ToString() => $"Return {Value} if {Condition}";
    public override T Accept<T>(IRVisitor<T> visitor) => visitor.VisitReturnIfInstruction(this);
}