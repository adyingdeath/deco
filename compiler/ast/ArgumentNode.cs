using Deco.Types;

namespace Deco.Compiler.Ast;

public class ArgumentNode(
    IType type, IdentifierNode name, int line = 0, int column = 0
) : AstNode(line, column) {
    public IType Type { get; } = type;
    public IdentifierNode Name { get; } = name;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitArgument(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public ArgumentNode With(IType? type = null, IdentifierNode? name = null) {
        var newNode = new ArgumentNode(
            type ?? this.Type,
            name ?? this.Name,
            this.Line,
            this.Column
        );
        return (ArgumentNode)newNode.CloneContext(this);
    }
}
