namespace Deco.Compiler.Ast;

public class IfNode(ExpressionNode condition, BlockNode thenBlock, BlockNode? elseBlock = null, int line = 0, int col = 0) : StatementNode(line, col) {
    public ExpressionNode Condition { get; } = condition;
    public BlockNode ThenBlock { get; } = thenBlock;
    public BlockNode? ElseBlock { get; } = elseBlock;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitIf(this);
    }

    public override IEnumerable<AstNode> GetChildren() {
        yield return Condition;
        yield return ThenBlock;
        if (ElseBlock != null) {
            yield return ElseBlock;
        }
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public IfNode With(
        ExpressionNode? condition = null,
        BlockNode? thenBlock = null,
        BlockNode? elseBlock = null
    ) {
        var newNode = new IfNode(
            condition ?? this.Condition,
            thenBlock ?? this.ThenBlock,
            elseBlock ?? this.ElseBlock,
            this.Line,
            this.Column
        );
        newNode.CloneContext(this);
        newNode.SetChildrenParent();
        return newNode;
    }
}
