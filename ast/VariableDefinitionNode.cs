using Deco.Types;

namespace Deco.Ast;

public class VariableDefinitionNode(IType type, IdentifierNode name, ExpressionNode? init, int line = 0, int column = 0) : StatementNode(line, column) {
    public IType Type { get; } = type;
    public IdentifierNode Name { get; } = name;
    public ExpressionNode? InitialValue { get; } = init;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitVariableDefinition(this);
    }
}
