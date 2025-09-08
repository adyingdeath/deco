namespace Deco.Ast;

public class BlockNode(List<StatementNode>? statements = null, int line = 0, int column = 0) : StatementNode(line, column) {
    public List<StatementNode> Statements { get; } = statements ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitBlock(this);
    }

    public override BlockNode Clone() {
        return new BlockNode(
            Statements.Select(s => (StatementNode)s.Clone()).ToList(),
            Line,
            Column
        );
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

    public override FakeBlockNode Clone() {
        return new FakeBlockNode(
            Statements.Select(s => (StatementNode)s.Clone()).ToList(),
            Line,
            Column
        );
    }
}
