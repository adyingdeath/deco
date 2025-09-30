namespace Deco.Compiler.IR;

/// <summary>
/// Move value from one operand to another
/// </summary>
public class MoveInstruction(
    Operand source, Operand dest
) : IRInstruction(IROpCode.Move) {
    public Operand Source { get; } = source;
    public Operand Destination { get; } = dest;
    public override string ToString() => $"Move {Source} => {Destination}";
}

/// <summary>
/// IR instruction for binary comparison operations.
/// </summary>
public class BinaryInstruction(
    IROpCode opCode, Operand dest, Operand left, Operand right
) : IRInstruction(opCode) {
    public Operand Destination { get; } = dest;
    public Operand Left { get; } = left;
    public Operand Right { get; } = right;
    public override string ToString() => $"{OpCode} {Left} {Right} => {Destination}";
}

/// <summary>
/// IR instruction for unary operations.
/// </summary>
public class UnaryInstruction(
    IROpCode opCode, Operand dest, Operand operand
) : IRInstruction(opCode) {
    public Operand Destination { get; } = dest;
    public Operand Operand { get; } = operand;
    public override string ToString() => $"{OpCode} {Operand} => {Destination}";

}

// Convenience classes for specific operations
public class AddInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.Add, dest, left, right);

public class SubtractInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.Subtract, dest, left, right);

public class MultiplyInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.Multiply, dest, left, right);

public class DivideInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.Divide, dest, left, right);

public class EqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.Equal, dest, left, right);

public class NotEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.NotEqual, dest, left, right);

public class LessThanInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.LessThan, dest, left, right);

public class LessThanOrEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.LessThanOrEqual, dest, left, right);

public class GreaterThanInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.GreaterThan, dest, left, right);

public class GreaterThanOrEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.GreaterThanOrEqual, dest, left, right);

public class LogicalAndInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.LogicalAnd, dest, left, right);

public class LogicalOrInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(IROpCode.LogicalOr, dest, left, right);

public class NegateInstruction(
    Operand dest, Operand operand
) : UnaryInstruction(IROpCode.Negate, dest, operand);

public class LogicalNotInstruction(
    Operand dest, Operand operand
) : UnaryInstruction(IROpCode.LogicalNot, dest, operand);
