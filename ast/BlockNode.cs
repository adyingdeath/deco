using System.Collections.Generic;

namespace Deco.Ast;

public class BlockNode(List<StatementNode>? statements = null, int line = 0, int column = 0) : AstNode(line, column) {
    public List<StatementNode> Statements { get; } = statements ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitBlock(this);
    }
}
