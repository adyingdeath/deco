namespace Deco.Ast;

public class AssignmentNode(string variable, ExpressionNode expression, int line = 0, int column = 0) : StatementNode(line, column) {
    public string Variable { get; } = variable;
    public ExpressionNode Expression { get; } = expression;

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitAssignment(this);
    }
}
