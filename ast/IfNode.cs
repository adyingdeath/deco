namespace Deco.Ast;

public class IfNode(ExpressionNode condition, BlockNode thenBlock, StatementNode? elseBlock = null, int line = 0, int col = 0) : StatementNode(line, col) {
    public ExpressionNode Condition { get; } = condition;
    public BlockNode ThenBlock { get; } = thenBlock;
    public StatementNode? ElseBlock { get; } = elseBlock;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitIf(this);
    }
}
