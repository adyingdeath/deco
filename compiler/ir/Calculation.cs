namespace Deco.Compiler.IR;

/// <summary>
/// Move value from one operand to another
/// </summary>
public class MoveInstruction(
    Operand source, Operand dest
) : IRInstruction {
    public Operand Source { get; } = source;
    public Operand Destination { get; } = dest;
    public override string ToString() => $"Move {Source} => {Destination}";
}

/// <summary>
/// IR instruction for binary comparison operations.
/// </summary>
public class BinaryInstruction(
    Operand dest, Operand left, Operand right
) : IRInstruction {
    public Operand Destination { get; } = dest;
    public Operand Left { get; } = left;
    public Operand Right { get; } = right;
    public override string ToString() => $"{Left} {Right} => {Destination}";
}

/// <summary>
/// IR instruction for unary operations.
/// </summary>
public class UnaryInstruction(
    Operand dest, Operand operand
) : IRInstruction {
    public Operand Destination { get; } = dest;
    public Operand Operand { get; } = operand;
    public override string ToString() => $"{Operand} => {Destination}";

}

// Convenience classes for specific operations
public class AddInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class SubtractInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class MultiplyInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class DivideInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class EqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class NotEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class LessThanInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class LessThanOrEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class GreaterThanInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class GreaterThanOrEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class LogicalAndInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class LogicalOrInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right);

public class NegateInstruction(
    Operand dest, Operand operand
) : UnaryInstruction(dest, operand);

public class LogicalNotInstruction(
    Operand dest, Operand operand
) : UnaryInstruction(dest, operand);
