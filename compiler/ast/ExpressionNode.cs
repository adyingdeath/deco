using Deco.Types;

namespace Deco.Compiler.Ast;

public abstract class ExpressionNode(int line = 0, int column = 0) : AstNode(line, column) {
    /// <summary>
    /// The type of this expression. Defaults to UnknownType for unresolved types.
    /// When set to a resolved type, it replaces the default UnknownType.
    /// </summary>
    public virtual IType Type { get; set; } = TypeUtils.UnknownType;
}
