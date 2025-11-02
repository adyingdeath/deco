namespace Deco.Compiler.Ast;

public class ReturnNode(ExpressionNode? expression = null, int line = 0, int column = 0) : StatementNode(line, column) {
    public ExpressionNode? Expression { get; } = expression;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitReturn(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        if (Expression != null) {
            yield return Expression;
        }
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public ReturnNode With(ExpressionNode? expression = null) {
        var newNode = new ReturnNode(
            expression ?? this.Expression,
            this.Line,
            this.Column
        );
        return (ReturnNode)newNode.CloneContext(this);
    }
}
