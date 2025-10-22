using Deco.Types;

namespace Deco.Compiler.Ast;

public class IdentifierNode(
    IType type, string name, int line = 0, int column = 0
) : ExpressionNode(type, line, column) {
    public string Name { get; } = name;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitIdentifier(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public IdentifierNode With(IType? type = null, string? name = null) {
        var newNode = new IdentifierNode(
            type ?? this.Type,
            name ?? this.Name,
            this.Line,
            this.Column
        );
        return (IdentifierNode)newNode.CloneContext(this);
    }
}
