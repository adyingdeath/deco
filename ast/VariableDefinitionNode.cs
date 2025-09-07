namespace Deco.Ast;

public class VariableDefinitionNode(string type, string name, ExpressionNode? init, int line = 0, int column = 0) : StatementNode(line, column) {
    public string Type { get; } = type;
    public string Name { get; } = name;
    public ExpressionNode? InitialValue { get; } = init;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitVariableDefinition(this);
    }
}
