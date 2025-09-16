using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Collect_Symbol;

/// <summary>
/// Pass to build symbol tables for nested scopes, including local variables.
/// This pass traverses all the block bodies and creates scopes for blocks,
/// including function, if, for, while, etc.
/// </summary>
public class ScopedSymbolTableBuilder(SymbolTable symbolTable) : IAstVisitor<object> {
    private readonly SymbolTable _symbolTable = symbolTable;
    private readonly List<string> _errors = [];

    public List<string> Errors => _errors;

    public object VisitProgram(ProgramNode node) {
        // Process each function's body
        foreach (var func in node.Functions) {
            func.Accept(this);
        }

        return null!;
    }

    public object VisitFunction(FunctionNode node) {
        // Enter function scope
        _symbolTable.EnterScope($"function {node.Name}");

        try {
            // Set the scope reference for this function node
            node.Scope = _symbolTable.CurrentScope;

            // Add function parameters to scope
            foreach (var arg in node.Arguments) {
                var argType = TypeUtils.ParseType(arg.Type);
                try {
                    _symbolTable.AddSymbol(new Symbol(
                        arg.Name,
                        argType,
                        SymbolKind.Parameter,
                        arg.Line,
                        arg.Column
                    ));
                } catch (SymbolTableException ex) {
                    _errors.Add($"Function '{node.Name}' parameter error: {ex.Message}");
                }
            }

            // Visit function body
            node.Body.Accept(this);
        } finally {
            // Exit function scope
            _symbolTable.ExitScope();
        }

        return null!;
    }

    public object VisitBlock(BlockNode node) {
        // Enter block scope
        _symbolTable.EnterScope("block");

        try {
            // Set the scope reference for this block node
            node.Scope = _symbolTable.CurrentScope;

            foreach (var stmt in node.Statements) {
                stmt.Accept(this);
            }
        } finally {
            // Exit block scope
            _symbolTable.ExitScope();
        }

        return null!;
    }

    public object VisitVariableDefinition(VariableDefinitionNode node) {
        var varDefType = TypeUtils.ParseType(node.Type);
        try {
            _symbolTable.AddSymbol(new Symbol(
                node.Name,
                varDefType,
                SymbolKind.Variable,
                node.Line,
                node.Column
            ));
        } catch (SymbolTableException ex) {
            _errors.Add($"Variable definition error: {ex.Message}");
        }

        return null!;
    }

    // Recursive traversal for nested blocks in control statements
    public object VisitIf(IfNode node) {
        // We don't need to visit condition part, 
        // because there shouldn't be new identifier definition in it
        // node.Condition.Accept(this);
        node.ThenBlock.Accept(this);
        node.ElseBlock?.Accept(this);
        return null!;
    }

    public object VisitWhile(WhileNode node) {
        // node.Condition.Accept(this);
        node.Body.Accept(this);
        return null!;
    }

    public object VisitFor(ForNode node) {
        // For loop initializer might define variables
        if (node.Initialization is VariableDefinitionNode init) {
            init.Accept(this);
        }
        if (node.Iteration is VariableDefinitionNode iter) {
            iter.Accept(this);
        }
        // Note: Other parts of for loop are visited but don't create new scopes
        // here, because the BlockNode itself will create scope.
        node.Body.Accept(this);
        return null!;
    }

    // Stub implementations for other nodes - they don't define symbols in this pass
    public object VisitModifier(ModifierNode node) => null!;
    public object VisitArgument(ArgumentNode node) => null!;
    public object VisitExpressionStatement(ExpressionStatementNode node) => null!;
    public object VisitCommand(CommandNode node) => null!;
    public object VisitAssignment(AssignmentNode node) => null!;
    public object VisitReturn(ReturnNode node) => null!;
    public object VisitFakeBlock(FakeBlockNode node) => null!;
    public object VisitBinaryOp(BinaryOpNode node) => null!;
    public object VisitUnaryOp(UnaryOpNode node) => null!;
    public object VisitLiteral(LiteralNode node) => null!;
    public object VisitIdentifier(IdentifierNode node) => null!;
    public object VisitFunctionCall(FunctionCallNode node) => null!;
}
