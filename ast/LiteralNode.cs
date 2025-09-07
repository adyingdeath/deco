namespace Deco.Ast;

public enum LiteralType {
    Number,
    String,
    Boolean,
    Null
}

public class LiteralNode(LiteralType type, object value, int line = 0, int column = 0) : ExpressionNode(line, column) {
    public LiteralType Type { get; } = type;
    public object Value { get; } = value;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitLiteral(this);
    }
}
