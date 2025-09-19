using Deco.Types;

namespace Deco.Ast;

public class LiteralNode(
    IType type, string value, int line = 0, int column = 0
) : ExpressionNode(type, line, column) {
    public string Value { get; } = value;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitLiteral(this);
    }
}
