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
    // Base ToString for binary operations, concrete instructions will override or prepend.
    public override string ToString() => $"Binary {Left} {Right} => {Destination}";
}

/// <summary>
/// IR instruction for unary operations.
/// </summary>
public class UnaryInstruction(
    Operand dest, Operand operand
) : IRInstruction {
    public Operand Destination { get; } = dest;
    public Operand Operand { get; } = operand;
    // Base ToString for unary operations, concrete instructions will override or prepend.
    public override string ToString() => $"Unary {Operand} => {Destination}";
}

// Convenience classes for specific operations
public class AddInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"Add {Left} {Right} => {Destination}";
}

public class SubtractInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"Subtract {Left} {Right} => {Destination}";
}

public class MultiplyInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"Multiply {Left} {Right} => {Destination}";
}

public class DivideInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"Divide {Left} {Right} => {Destination}";
}

public class EqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"Equal {Left} {Right} => {Destination}";
}

public class NotEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"NotEqual {Left} {Right} => {Destination}";
}

public class LessThanInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"LessThan {Left} {Right} => {Destination}";
}

public class LessThanOrEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"LessThanOrEqual {Left} {Right} => {Destination}";
}

public class GreaterThanInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"GreaterThan {Left} {Right} => {Destination}";
}

public class GreaterThanOrEqualInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"GreaterThanOrEqual {Left} {Right} => {Destination}";
}

public class LogicalAndInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"LogicalAnd {Left} {Right} => {Destination}";
}

public class LogicalOrInstruction(
    Operand dest, Operand left, Operand right
) : BinaryInstruction(dest, left, right) {
    public override string ToString() => $"LogicalOr {Left} {Right} => {Destination}";
}

public class NegateInstruction(
    Operand dest, Operand operand
) : UnaryInstruction(dest, operand) {
    public override string ToString() => $"Negate {Operand} => {Destination}";
}

public class LogicalNotInstruction(
    Operand dest, Operand operand
) : UnaryInstruction(dest, operand) {
    public override string ToString() => $"LogicalNot {Operand} => {Destination}";
}