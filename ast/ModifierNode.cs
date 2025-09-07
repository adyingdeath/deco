using System.Collections.Generic;

namespace Deco.Ast;

public class ModifierNode(string name, List<ExpressionNode>? parameters = null, int line = 0, int column = 0) : AstNode(line, column) {
    public string Name { get; } = name;
    public List<ExpressionNode> Parameters { get; } = parameters ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitModifier(this);
    }
}
