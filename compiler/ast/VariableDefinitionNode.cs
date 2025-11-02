namespace Deco.Compiler.Ast;

public class VariableDefinitionNode(IdentifierNode name, ExpressionNode? init, int line = 0, int column = 0) : StatementNode(line, column) {
    public IdentifierNode Name { get; } = name;
    public ExpressionNode? InitialValue { get; } = init;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitVariableDefinition(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        yield return Name;
        if (InitialValue != null) {
            yield return InitialValue;
        }
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public VariableDefinitionNode With(
        IdentifierNode? name = null,
        ExpressionNode? initialValue = null
    ) {
        var newNode = new VariableDefinitionNode(
            name ?? this.Name,
            initialValue ?? this.InitialValue,
            this.Line,
            this.Column
        );
        return (VariableDefinitionNode)newNode.CloneContext(this);
    }
}
