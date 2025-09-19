using Deco.Types;

namespace Deco.Ast;

public enum UnaryOperator {
    Negate,
    LogicalNot
}

public class UnaryOpNode(
    UnaryOperator op, ExpressionNode operand, int line = 0, int column = 0
) : ExpressionNode(TypeUtils.VoidType, line, column) {
    public UnaryOperator Operator { get; } = op;
    public ExpressionNode Operand { get; } = operand;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitUnaryOp(this);
    }
}
