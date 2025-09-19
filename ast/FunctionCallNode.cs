using System.Collections.Generic;
using Deco.Types;

namespace Deco.Ast;

public class FunctionCallNode(
    IdentifierNode name, List<ExpressionNode>? arguments = null, int line = 0, int column = 0
) : ExpressionNode(TypeUtils.VoidType, line, column) {
    public IdentifierNode Name { get; } = name;
    public List<ExpressionNode> Arguments { get; } = arguments ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFunctionCall(this);
    }
}
