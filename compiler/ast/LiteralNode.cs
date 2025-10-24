using Deco.Compiler.Types;

namespace Deco.Compiler.Ast;

public class LiteralNode(
    IType type, string value, int line = 0, int column = 0
) : ExpressionNode(type, line, column) {
    public string Value { get; } = value;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitLiteral(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public LiteralNode With(IType? type = null, string? value = null) {
        var newNode = new LiteralNode(
            type ?? this.Type,
            value ?? this.Value,
            this.Line,
            this.Column
        );
        return (LiteralNode)newNode.CloneContext(this);
    }
}
