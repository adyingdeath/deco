namespace Deco.Compiler.Ast;

public class BlockNode(List<StatementNode>? statements = null, int line = 0, int column = 0) : StatementNode(line, column) {
    public List<StatementNode> Statements { get; } = statements ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitBlock(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        foreach (var statement in Statements) {
            yield return statement;
        }
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public BlockNode With(List<StatementNode>? statements = null) {
        var newNode = new BlockNode(
            statements ?? [.. this.Statements],
            this.Line,
            this.Column
        );
        newNode.CloneContext(this);
        newNode.SetChildrenParent();
        return newNode;
    }
}
