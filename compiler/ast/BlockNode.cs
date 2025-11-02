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
        return (BlockNode)newNode.CloneContext(this);
    }
}

// <summary>
// Used to expand a for-loop into a while-loop. This special block will be flattened later.
// </summary>
public class FakeBlockNode(List<StatementNode>? statements = null, int line = 0, int column = 0) : StatementNode(line, column) {
    public List<StatementNode> Statements { get; } = statements ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFakeBlock(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public FakeBlockNode With(List<StatementNode>? statements = null) {
        var newNode = new FakeBlockNode(
            statements ?? [.. this.Statements],
            this.Line,
            this.Column
        );
        return (FakeBlockNode)newNode.CloneContext(this);
    }
}
