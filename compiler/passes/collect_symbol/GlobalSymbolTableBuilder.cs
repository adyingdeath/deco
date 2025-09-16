using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Collect_Symbol;

/// <summary>
/// Pass to build the global symbol table by collecting global variable and
/// function declarations.
/// This pass populates the global scope with symbols.
/// </summary>
public class GlobalSymbolTableBuilder(SymbolTable symbolTable) : IAstVisitor<object> {
    private readonly SymbolTable _symbolTable = symbolTable;
    private readonly List<string> _errors = [];

    public List<string> Errors => _errors;

    public object VisitProgram(ProgramNode node) {
        // Set the global scope reference for the program node
        node.Scope = _symbolTable.GlobalScope;

        // First, collect global variable definitions
        foreach (var varDef in node.VariableDefinitions) {
            varDef.Accept(this);
        }

        // Then, collect function definitions (but don't enter their bodies yet)
        foreach (var func in node.Functions) {
            func.Accept(this);
        }

        return null!;
    }

    public object VisitFunction(FunctionNode node) {
        // For global scope, we only care about the function signature, not the body
        var returnType = TypeUtils.ParseType(node.ReturnType);
        var parameterTypes = node.Arguments
            .Select(arg => TypeUtils.ParseType(arg.Type)).ToList();
        var functionType = new FunctionType(returnType, parameterTypes);

        try {
            _symbolTable.AddSymbol(new Symbol(
                node.Name,
                functionType,
                SymbolKind.Function,
                node.Line,
                node.Column
            ));
        } catch (SymbolTableException ex) {
            _errors.Add($"Global symbol error: {ex.Message}");
        }

        return null!;
    }

    public object VisitVariableDefinition(VariableDefinitionNode node) {
        var symbolType = TypeUtils.ParseType(node.Type);
        try {
            _symbolTable.AddSymbol(new Symbol(
                node.Name,
                symbolType,
                SymbolKind.Variable,
                node.Line,
                node.Column
            ));
        } catch (SymbolTableException ex) {
            _errors.Add($"Global symbol error: {ex.Message}");
        }

        return null!;
    }

    // Stub implementations for interface compliance - not called in this pass
    public object VisitModifier(ModifierNode node) => null!;
    public object VisitArgument(ArgumentNode node) => null!;
    public object VisitExpressionStatement(ExpressionStatementNode node) => null!;
    public object VisitCommand(CommandNode node) => null!;
    public object VisitAssignment(AssignmentNode node) => null!;
    public object VisitReturn(ReturnNode node) => null!;
    public object VisitIf(IfNode node) => null!;
    public object VisitWhile(WhileNode node) => null!;
    public object VisitFor(ForNode node) => null!;
    public object VisitBlock(BlockNode node) => null!;
    public object VisitFakeBlock(FakeBlockNode node) => null!;
    public object VisitBinaryOp(BinaryOpNode node) => null!;
    public object VisitUnaryOp(UnaryOpNode node) => null!;
    public object VisitLiteral(LiteralNode node) => null!;
    public object VisitIdentifier(IdentifierNode node) => null!;
    public object VisitFunctionCall(FunctionCallNode node) => null!;
}
