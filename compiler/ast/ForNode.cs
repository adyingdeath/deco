namespace Deco.Compiler.Ast;

public class ForNode(
    StatementNode? initialization,
    ExpressionNode? condition,
    StatementNode? iteration,
    BlockNode body, int line = 0, int column = 0
) : StatementNode(line, column) {
    public StatementNode? Initialization { get; } = initialization;
    public ExpressionNode? Condition { get; } = condition;
    public StatementNode? Iteration { get; } = iteration;
    public BlockNode Body { get; } = body;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFor(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public ForNode With(
        StatementNode? initialization = null,
        ExpressionNode? condition = null,
        StatementNode? iteration = null,
        BlockNode? body = null
    ) {
        var newNode = new ForNode(
            initialization ?? this.Initialization,
            condition ?? this.Condition,
            iteration ?? this.Iteration,
            body ?? this.Body,
            this.Line,
            this.Column
        );
        return (ForNode)newNode.CloneContext(this);
    }
}
