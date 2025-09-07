using System.Collections.Generic;

namespace Deco.Ast;

public class FunctionCallNode(string name, List<ExpressionNode>? arguments = null, int line = 0, int column = 0) : ExpressionNode(line, column) {
    public string Name { get; } = name;
    public List<ExpressionNode> Arguments { get; } = arguments ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFunctionCall(this);
    }
}
