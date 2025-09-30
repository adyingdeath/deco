namespace Deco.Compiler.Ast;

public class ProgramNode(
    List<VariableDefinitionNode>? variableDefinitions,
    List<FunctionNode>? functions,
    int line = 0, int column = 0
) : AstNode(line, column) {
    public List<VariableDefinitionNode> VariableDefinitions { get; }
        = variableDefinitions ?? [];
    public List<FunctionNode> Functions { get; } = functions ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitProgram(this);
    }
}
