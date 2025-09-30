using System.Collections.Generic;
using Deco.Types;

namespace Deco.Compiler.Ast;

public class FunctionCallNode(
    IdentifierNode name, List<ExpressionNode>? arguments = null, int line = 0, int column = 0
) : ExpressionNode(line, column) {
    public IdentifierNode Name { get; } = name;
    public List<ExpressionNode> Arguments { get; } = arguments ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFunctionCall(this);
    }
}
