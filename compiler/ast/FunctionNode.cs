using Deco.Types;

namespace Deco.Compiler.Ast;

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

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public FunctionNode With(
        List<ModifierNode>? modifiers = null,
        IType? returnType = null,
        IdentifierNode? name = null,
        List<ArgumentNode>? arguments = null,
        BlockNode? body = null
    ) {
        var newNode = new FunctionNode(
            modifiers ?? [.. this.Modifiers],
            returnType ?? this.ReturnType,
            name ?? this.Name,
            arguments ?? [.. this.Arguments],
            body ?? this.Body,
            this.Line,
            this.Column
        );
        return (FunctionNode)newNode.CloneContext(this);
    }
}
