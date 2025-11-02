using Deco.Compiler.Types;

namespace Deco.Compiler.Ast;

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

public class BinaryOpNode(
    IType type, ExpressionNode left, BinaryOperator op, ExpressionNode right,
    int line = 0, int column = 0
) : ExpressionNode(type, line, column) {
    public ExpressionNode Left { get; } = left;
    public BinaryOperator Operator { get; } = op;
    public ExpressionNode Right { get; } = right;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitBinaryOp(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        yield return Left;
        yield return Right;
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public BinaryOpNode With(
        IType? type = null,
        ExpressionNode? left = null,
        BinaryOperator? op = null,
        ExpressionNode? right = null
    ) {
        var newNode = new BinaryOpNode(
            type ?? this.Type,
            left ?? this.Left,
            op ?? this.Operator,
            right ?? this.Right,
            this.Line,
            this.Column
        );
        return (BinaryOpNode)newNode.CloneContext(this);
    }
}
