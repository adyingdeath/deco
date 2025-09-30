namespace Deco.Compiler.Ast;

public class WhileNode(ExpressionNode condition, BlockNode body, int line = 0, int column = 0) : StatementNode(line, column) {
    public ExpressionNode Condition { get; } = condition;
    public BlockNode Body { get; } = body;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitWhile(this);
    }
}
