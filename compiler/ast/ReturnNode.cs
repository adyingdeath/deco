namespace Deco.Compiler.Ast;

public class ReturnNode(ExpressionNode? expression = null, int line = 0, int column = 0) : StatementNode(line, column) {
    public ExpressionNode? Expression { get; } = expression;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitReturn(this);
    }
}
