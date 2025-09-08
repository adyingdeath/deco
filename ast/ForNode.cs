namespace Deco.Ast;

public class ForNode(
    StatementNode? initialization,
    ExpressionNode condition,
    StatementNode? iteration,
    BlockNode body, int line = 0, int column = 0
) : StatementNode(line, column) {
    public StatementNode? Initialization { get; } = initialization;
    public ExpressionNode Condition { get; } = condition;
    public StatementNode? Iteration { get; } = iteration;
    public BlockNode Body { get; } = body;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFor(this);
    }

    public override ForNode Clone() {
        return new ForNode(
            Initialization != null ? (StatementNode)Initialization.Clone() : null,
            Condition.Clone(),
            Iteration != null ? (StatementNode)Iteration.Clone() : null,
            Body.Clone(),
            Line,
            Column
        );
    }
}
