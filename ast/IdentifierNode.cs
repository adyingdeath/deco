using Deco.Types;

namespace Deco.Ast;

public class IdentifierNode(
    string name, int line = 0, int column = 0
) : ExpressionNode(TypeUtils.VoidType, line, column) {
    public string Name { get; } = name;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitIdentifier(this);
    }
}
