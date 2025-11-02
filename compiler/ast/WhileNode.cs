namespace Deco.Compiler.Ast;

public class WhileNode(ExpressionNode condition, BlockNode body, int line = 0, int column = 0) : StatementNode(line, column) {
    public ExpressionNode Condition { get; } = condition;
    public BlockNode Body { get; } = body;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitWhile(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        yield return Condition;
        yield return Body;
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public WhileNode With(ExpressionNode? condition = null, BlockNode? body = null) {
        var newNode = new WhileNode(
            condition ?? this.Condition,
            body ?? this.Body,
            this.Line,
            this.Column
        );
        return (WhileNode)newNode.CloneContext(this);
    }
}
