namespace Deco.Compiler.Ast;

public class ArgumentNode(
    IdentifierNode name, int line = 0, int column = 0
) : AstNode(line, column) {
    public IdentifierNode Name { get; } = name;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitArgument(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public ArgumentNode With(IdentifierNode? name = null) {
        var newNode = new ArgumentNode(
            name ?? this.Name,
            this.Line,
            this.Column
        );
        return (ArgumentNode)newNode.CloneContext(this);
    }
}
