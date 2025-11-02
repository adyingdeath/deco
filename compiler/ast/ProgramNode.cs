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

    public override IEnumerable<AstNode> GetChildren() {
        foreach (var variableDefinition in VariableDefinitions) {
            yield return variableDefinition;
        }
        foreach (var function in Functions) {
            yield return function;
        }
    }

    /// <summary>
    /// Creates a new Node that is a copy of the current one,
    /// but with the specified properties replaced.
    /// </summary>
    public ProgramNode With(
        List<VariableDefinitionNode>? variableDefinitions = null,
        List<FunctionNode>? functions = null
    ) {
        var newNode = new ProgramNode(
            variableDefinitions ?? [.. this.VariableDefinitions],
            functions ?? [.. this.Functions],
            this.Line,
            this.Column
        );
        newNode.CloneContext(this);
        newNode.SetChildrenParent();
        return newNode;
    }
}
