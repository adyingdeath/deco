namespace Deco.Ast;

public class ReturnNode(ExpressionNode? expression = null, int line = 0, int column = 0) : StatementNode(line, column) {
    public ExpressionNode? Expression { get; } = expression;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitReturn(this);
    }

    public override ReturnNode Clone() {
        return new ReturnNode(
            Expression?.Clone() != null ? Expression.Clone() : null,
            Line,
            Column
        );
    }
}
