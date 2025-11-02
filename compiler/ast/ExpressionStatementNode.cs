namespace Deco.Compiler.Ast;

public class ExpressionStatementNode(ExpressionNode expression, int line = 0, int column = 0) : StatementNode(line, column) {
    public ExpressionNode Expression { get; } = expression;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitExpressionStatement(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        yield return Expression;
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public ExpressionStatementNode With(ExpressionNode? expression = null) {
        var newNode = new ExpressionStatementNode(
            expression ?? this.Expression,
            this.Line,
            this.Column
        );
        newNode.CloneContext(this);
        newNode.SetChildrenParent();
        return newNode;
    }
}
