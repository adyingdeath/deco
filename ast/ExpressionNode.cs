using Deco.Types;

namespace Deco.Ast;

public abstract class ExpressionNode(IType type, int line = 0, int column = 0) : AstNode(line, column) {
    public virtual IType Type { get; set; } = type;
}
