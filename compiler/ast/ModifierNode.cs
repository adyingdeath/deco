namespace Deco.Compiler.Ast;

public class ModifierNode(IdentifierNode name, List<ExpressionNode>? parameters = null, int line = 0, int column = 0) : AstNode(line, column) {
    public IdentifierNode Name { get; } = name;
    public List<ExpressionNode> Parameters { get; } = parameters ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitModifier(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        yield return Name;
        foreach (var parameter in Parameters) {
            yield return parameter;
        }
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public ModifierNode With(
        IdentifierNode? name = null,
        List<ExpressionNode>? parameters = null
    ) {
        var newNode = new ModifierNode(
            name ?? this.Name,
            parameters ?? [.. this.Parameters],
            this.Line,
            this.Column
        );
        newNode.CloneContext(this);
        newNode.SetChildrenParent();
        return newNode;
    }
}
