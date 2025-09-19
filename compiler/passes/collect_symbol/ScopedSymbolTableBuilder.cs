using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Collect_Symbol;

///// <summary>
///// Pass to build symbol tables for nested functions and blocks, including local variables.
/// This pass traverses all the block bodies and creates symbol tables for functions and blocks.
/// </summary>
public class ScopedSymbolTableBuilder(Scope globalSymbolTable) : IAstVisitor<object> {
    private readonly ScopeStack scope = new(globalSymbolTable);
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
        // Create a symbol table for this function with the current table as parent
        node.Scope = scope.Current().CreateChild($"function {node.Name}");

        // Add function parameters to the function's symbol table
        foreach (var arg in node.Arguments) {
            var argType = TypeUtils.ParseType(arg.Type);
            try {
                node.Scope.AddSymbol(new Symbol(
                    arg.Name,
                    Compiler.variableCodeGen.Next(),
                    argType,
                    SymbolKind.Parameter,
                    arg.Line,
                    arg.Column
                ));
            } catch (SymbolTableException ex) {
                _errors.Add($"Function '{node.Name}' parameter error: {ex.Message}");
            }
        }

        // Switch to function symbol table for its body
        scope.PushScope(node.Scope);

        // Visit function body
        node.Body.Accept(this);

        // Restore previous symbol table
        scope.PopScope();

        return null!;
    }

    public object VisitBlock(BlockNode node) {
        // Create a symbol table for this block with current table as parent
        node.Scope = scope.Current().CreateChild("block");

        // Switch to block symbol table for its statements
        scope.PushScope(node.Scope);

        foreach (var stmt in node.Statements) {
            stmt.Accept(this);
        }

        // Restore previous symbol table
        scope.PopScope();

        return null!;
    }

    public object VisitVariableDefinition(VariableDefinitionNode node) {
        // Since VariableDefinitionNode no longer has Type, we need to find the expected type
        // This can be done by looking for type annotations that should be provided separately
        // For now, store as UnknownType and let type inference handle it
        try {
            scope.Current().AddSymbol(new Symbol(
                node.Name.Name,
                Compiler.variableCodeGen.Next(),
                TypeUtils.UnknownType,
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
        node.Scope = scope.Current().CreateChild("for loop");
        scope.PushScope(node.Scope);
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
        scope.PopScope();
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
