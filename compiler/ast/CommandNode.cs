namespace Deco.Compiler.Ast;

public class CommandNode(string command, int line = 0, int column = 0) : StatementNode(line, column) {
    public string Command { get; } = command;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitCommand(this);
    }
}
