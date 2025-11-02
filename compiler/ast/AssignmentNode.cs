namespace Deco.Compiler.Ast;

public class AssignmentNode(
    IdentifierNode variable, ExpressionNode expression,
    int line = 0, int column = 0
) : StatementNode(line, column) {
    public IdentifierNode Variable { get; } = variable;
    public ExpressionNode Expression { get; } = expression;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitAssignment(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        yield return Variable;
        yield return Expression;
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public AssignmentNode With(
        IdentifierNode? variable = null,
        ExpressionNode? expression = null
    ) {
        var newNode = new AssignmentNode(
            variable ?? this.Variable,
            expression ?? this.Expression,
            this.Line,
            this.Column
        );
        return (AssignmentNode)newNode.CloneContext(this);
    }
}
