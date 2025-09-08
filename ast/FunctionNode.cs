using System.Collections.Generic;

namespace Deco.Ast;

public class FunctionNode(
    List<ModifierNode> modifiers,
    string returnType,
    string name,
    List<ArgumentNode> arguments,
    BlockNode body,
    int line = 0,
    int column = 0
) : AstNode(line, column) {
    public List<ModifierNode> Modifiers { get; } = modifiers ?? new List<ModifierNode>();
    public string ReturnType { get; } = returnType;
    public string Name { get; } = name;
    public List<ArgumentNode> Arguments { get; } = arguments ?? new List<ArgumentNode>();
    public BlockNode Body { get; } = body;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFunction(this);
    }

    public override FunctionNode Clone() {
        return new FunctionNode(
            Modifiers.Select(m => m.Clone()).ToList(),
            ReturnType,
            Name,
            Arguments.Select(a => a.Clone()).ToList(),
            Body.Clone(),
            Line,
            Column
        );
    }
}
