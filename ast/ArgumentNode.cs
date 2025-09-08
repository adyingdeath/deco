namespace Deco.Ast;

public class ArgumentNode(string type, string name, int line = 0, int column = 0) : AstNode(line, column) {
    public string Type { get; } = type;
    public string Name { get; } = name;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitArgument(this);
    }

    public override ArgumentNode Clone() {
        return new ArgumentNode(
            Type,
            Name,
            Line,
            Column
        );
    }
}
