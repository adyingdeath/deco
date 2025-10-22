namespace Deco.Compiler.Ast;

public class CommandNode(string command, int line = 0, int column = 0) : StatementNode(line, column) {
    public string Command { get; } = command;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitCommand(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public CommandNode With(string? command = null) {
        var newNode = new CommandNode(
            command ?? this.Command,
            this.Line,
            this.Column
        );
        return (CommandNode)newNode.CloneContext(this);
    }
}
