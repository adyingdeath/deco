using Deco.Types;

namespace Deco.Compiler.Ast;

public enum UnaryOperator {
    Negate,
    LogicalNot
}

public class UnaryOpNode(
    IType type, UnaryOperator op, ExpressionNode operand, int line = 0, int column = 0
) : ExpressionNode(type, line, column) {
    public UnaryOperator Operator { get; } = op;
    public ExpressionNode Operand { get; } = operand;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitUnaryOp(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public UnaryOpNode With(
        IType? type = null,
        UnaryOperator? op = null,
        ExpressionNode? operand = null
    ) {
        var newNode = new UnaryOpNode(
            type ?? this.Type,
            op ?? this.Operator,
            operand ?? this.Operand,
            this.Line,
            this.Column
        );
        return (UnaryOpNode)newNode.CloneContext(this);
    }
}
