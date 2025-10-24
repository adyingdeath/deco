using System.Collections.Generic;
using Deco.Compiler.Types;

namespace Deco.Compiler.Ast;

public class FunctionCallNode(
    IType type, IdentifierNode name, List<ExpressionNode>? arguments = null, int line = 0, int column = 0
) : ExpressionNode(type, line, column) {
    public IdentifierNode Name { get; } = name;
    public List<ExpressionNode> Arguments { get; } = arguments ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFunctionCall(this);
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public FunctionCallNode With(
        IType? type = null,
        IdentifierNode? name = null,
        List<ExpressionNode>? arguments = null
    ) {
        var newNode = new FunctionCallNode(
            type ?? this.Type,
            name ?? this.Name,
            arguments ?? [.. this.Arguments],
            this.Line,
            this.Column
        );
        return (FunctionCallNode)newNode.CloneContext(this);
    }
}
