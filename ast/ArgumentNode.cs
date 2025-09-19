namespace Deco.Ast;

public class ArgumentNode(string type, IdentifierNode name, int line = 0, int column = 0) : AstNode(line, column) {
    public string Type { get; } = type;
    public IdentifierNode Name { get; } = name;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitArgument(this);
    }
}
