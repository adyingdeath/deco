namespace Deco.Ast;

public class ExpressionStatementNode(ExpressionNode expression, int line = 0, int column = 0) : StatementNode(line, column) {
    public ExpressionNode Expression { get; } = expression;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return default!;
    }
}
