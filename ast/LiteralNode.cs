using Deco.Types;

namespace Deco.Ast;

public class LiteralNode : ExpressionNode {
    public string Value { get; }

    public LiteralNode(
        IType type, string value, int line = 0, int column = 0
    ) : base(line, column) {
        this.Value = value;
        this.Type = type;
    }

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitLiteral(this);
    }
}
