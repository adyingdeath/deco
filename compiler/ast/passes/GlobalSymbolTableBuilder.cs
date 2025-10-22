using Deco.Compiler.Ast;
using Deco.Types;

namespace Deco.Compiler.Ast.Passes;

/// <summary>
/// Pass to build the global symbol table by collecting global variable and
/// function declarations.
/// This pass populates the global symbol table with symbols.
/// </summary>
public class GlobalSymbolTableBuilder(Scope symbolTable) : IAstVisitor<object> {
    private readonly Scope _symbolTable = symbolTable;
    private readonly List<string> _errors = [];

    public List<string> Errors => _errors;

    public static void Action(Scope symbolTable, AstNode astNode) {
        var gstBuilder = new GlobalSymbolTableBuilder(symbolTable);
        astNode.Accept(gstBuilder);
        if (gstBuilder.Errors.Count != 0) {
            Console.WriteLine("Global symbol table errors:");
            foreach (var error in gstBuilder.Errors) {
                Console.WriteLine($"  {error}");
            }
        }
    }

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
            // arg.Name.Type is UnresolvedType with the raw string name (e.g., "int")
            parameterTypes.Add(arg.Name.Type);
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
        /* We've stored an UnresolvedType with the raw string name (e.g., "int")
        into the node.Name.Type. We will directly use the type here. The type will
        be resolved later in TypeResolver. It will try to parse the type according
        to the UnresolvedType here. */
        try {
            _symbolTable.AddSymbol(new Symbol(
                node.Name.Name,
                Compiler.variableCodeGen.Next(),
                // This is an UnresolvedType
                node.Name.Type,
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
