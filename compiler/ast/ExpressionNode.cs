using Deco.Compiler.Types;

namespace Deco.Compiler.Ast;

public abstract class ExpressionNode(IType type, int line = 0, int column = 0) : AstNode(line, column) {
    /// <summary>
    /// The type of this expression.
    /// </summary>
    public virtual IType Type { get; set; } = type;
}
