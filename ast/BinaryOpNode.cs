namespace Deco.Ast;

public enum BinaryOperator {
    // Arithmetic
    Add,
    Subtract,
    Multiply,
    Divide,

    // Comparison
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,

    // Logical
    LogicalAnd,
    LogicalOr
}

public class BinaryOpNode(ExpressionNode left, BinaryOperator op, ExpressionNode right, int line = 0, int column = 0) : ExpressionNode(line, column) {
    public ExpressionNode Left { get; } = left;
    public BinaryOperator Operator { get; } = op;
    public ExpressionNode Right { get; } = right;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitBinaryOp(this);
    }
}
