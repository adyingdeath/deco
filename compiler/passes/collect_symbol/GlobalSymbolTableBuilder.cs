using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Collect_Symbol;

/// <summary>
/// Pass to build the global symbol table by collecting global variable and
/// function declarations.
/// This pass populates the global symbol table with symbols.
/// </summary>
public class GlobalSymbolTableBuilder(Scope symbolTable) : IAstVisitor<object> {
    private readonly Scope _symbolTable = symbolTable;
    private readonly List<string> _errors = [];

    public List<string> Errors => _errors;

    public object VisitProgram(ProgramNode node) {
        // Set the global symbol table reference for the program node
        node.Scope = _symbolTable;

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
        // For global table, we use raw UnresolvedTypes - parsing will be done later in TypeResolver
        var parameterTypes = new List<IType>();
        foreach (var arg in node.Arguments) {
            // arg.Type is the raw string name (e.g., "int"), keep as UnresolvedType
            var unresolvedParamType = new UnresolvedType(arg.Type);
            parameterTypes.Add(unresolvedParamType);
        }

        // node.ReturnType is UnresolvedType from AST builder
        var functionType = new FunctionType(node.ReturnType, parameterTypes);

        try {
            _symbolTable.AddSymbol(new FunctionSymbol(
                node.Name.Name,
                Compiler.functionCodeGen.Next(8),
                functionType,
                [],
                Symbol.Uninitialized,
                node.Line,
                node.Column
            ));
        } catch (SymbolTableException ex) {
            _errors.Add($"Global symbol error: {ex.Message}");
        }

        return null!;
    }

    public object VisitVariableDefinition(VariableDefinitionNode node) {
        // We can't determine the variable type from VariableDefinitionNode alone anymore
        // The type should be resolved from the initializer expression, but that's circular
        // in the current pipeline. For now, we'll store UnknownType and let the type resolver handle it.
        try {
            _symbolTable.AddSymbol(new Symbol(
                node.Name.Name,
                Compiler.variableCodeGen.Next(),
                TypeUtils.UnknownType,  // Will be resolved during type checking
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
