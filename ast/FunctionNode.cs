using Deco.Types;

namespace Deco.Ast;

public class FunctionNode(
    List<ModifierNode> modifiers,
    IType returnType,
    IdentifierNode name,
    List<ArgumentNode> arguments,
    BlockNode body,
    int line = 0,
    int column = 0
) : AstNode(line, column) {
    public List<ModifierNode> Modifiers { get; } = modifiers ?? new List<ModifierNode>();
    public IType ReturnType { get; } = returnType;
    public IdentifierNode Name { get; } = name;
    public List<ArgumentNode> Arguments { get; } = arguments ?? new List<ArgumentNode>();
    public BlockNode Body { get; } = body;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitFunction(this);
    }
}
