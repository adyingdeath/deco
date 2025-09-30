using System.Collections.Generic;

namespace Deco.Compiler.Ast;

public class ModifierNode(IdentifierNode name, List<ExpressionNode>? parameters = null, int line = 0, int column = 0) : AstNode(line, column) {
    public IdentifierNode Name { get; } = name;
    public List<ExpressionNode> Parameters { get; } = parameters ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitModifier(this);
    }
}
