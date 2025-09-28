namespace Deco.Compiler.IR;

public enum ConditionType {
    Equal,
    GreaterEqual,
    Greater,
    LessEqual,
    Less,
}

public class Condition(
    ConditionType type, Operand left, Operand right
) {
    public ConditionType Type { get; } = type;
    public Operand Left { get; } = left;
    public Operand Right { get; } = right;
}