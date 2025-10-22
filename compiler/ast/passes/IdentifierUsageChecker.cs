using Deco.Types;

namespace Deco.Compiler.Ast.Passes;

/// <summary>
/// Pass to check for undefined identifiers.
/// This pass traverses the AST and verifies that every IdentifierNode
/// references a symbol that exists in the symbol table.
/// </summary>
public class IdentifierUsageChecker(Scope globalSymbolTable) : IAstVisitor<object> {
    private readonly ScopeStack scope = new(globalSymbolTable);
    private readonly List<string> _errors = [];

    public List<string> Errors => _errors;

    public static void Action(Scope symbolTable, AstNode astNode) {
        var usageChecker = new IdentifierUsageChecker(symbolTable);
        astNode.Accept(usageChecker);
        if (usageChecker.Errors.Count != 0) {
            Console.WriteLine("Identifier usage errors:");
            foreach (var error in usageChecker.Errors) {
                Console.WriteLine($"  {error}");
            }
        }
    }

    private void CheckIdentifier(string name, int line, int column) {
        var symbol = scope.Current().LookupSymbol(name);
        if (symbol == null) {
            _errors.Add($"Undefined identifier '{name}' at line {line}, column {column}");
        }
    }

    public void Visit(AstNode astNode) {
        astNode.Accept(this);
    }

    public object VisitProgram(ProgramNode node) {
        foreach (var stmt in node.VariableDefinitions) {
            stmt.Accept(this);
        }
        foreach (var func in node.Functions) {
            func.Accept(this);
        }
        return null!;
    }

    public object VisitFunction(FunctionNode node) {
        scope.PushScope(node.Scope);
        node.Body.Accept(this);
        scope.PopScope();
        return null!;
    }

    public object VisitBlock(BlockNode node) {
        scope.PushScope(node.Scope);
        foreach (var stmt in node.Statements) {
            stmt.Accept(this);
        }
        scope.PopScope();
        return null!;
    }

    public object VisitExpressionStatement(ExpressionStatementNode node) {
        node.Expression.Accept(this);
        return null!;
    }

    public object VisitAssignment(AssignmentNode node) {
        // Check if the variable on the left side is defined
        CheckIdentifier(node.Variable.Name, node.Line, node.Column);

        // Check the right side expression
        node.Expression.Accept(this);
        return null!;
    }

    public object VisitReturn(ReturnNode node) {
        node.Expression?.Accept(this);
        return null!;
    }

    public object VisitIf(IfNode node) {
        // We don't need to PushScope here because there will be no new
        // identifier defined in condition expression(it's not a statement so 
        // you can't define new identifier in it), which means we didn't create
        // scope for the IF node.
        node.Condition.Accept(this);
        node.ThenBlock.Accept(this);
        node.ElseBlock?.Accept(this);
        return null!;
    }

    public object VisitWhile(WhileNode node) {
        scope.PushScope(node.Scope);
        node.Condition.Accept(this);
        node.Body.Accept(this);
        scope.PopScope();
        return null!;
    }

    public object VisitFor(ForNode node) {
        scope.PushScope(node.Scope);
        node.Initialization?.Accept(this);
        node.Condition?.Accept(this);
        node.Iteration?.Accept(this);
        node.Body.Accept(this);
        scope.PopScope();
        return null!;
    }

    public object VisitBinaryOp(BinaryOpNode node) {
        node.Left.Accept(this);
        node.Right.Accept(this);
        return null!;
    }

    public object VisitUnaryOp(UnaryOpNode node) {
        node.Operand.Accept(this);
        return null!;
    }

    public object VisitFunctionCall(FunctionCallNode node) {
        // Check if the function name is defined
        CheckIdentifier(node.Name.Name, node.Line, node.Column);

        // Check arguments
        foreach (var arg in node.Arguments) {
            arg.Accept(this);
        }
        return null!;
    }

    public object VisitIdentifier(IdentifierNode node) {
        // Check if this identifier is defined in its scope
        CheckIdentifier(node.Name, node.Line, node.Column);
        return null!;
    }

    public object VisitVariableDefinition(VariableDefinitionNode node) {
        // Visit the initializer if present
        node.InitialValue?.Accept(this);
        return null!;
    }
    // Stub implementations for interface compliance
    public object VisitModifier(ModifierNode node) => null!;
    public object VisitArgument(ArgumentNode node) => null!;
    public object VisitCommand(CommandNode node) => null!;
    public object VisitLiteral(LiteralNode node) => null!;
    public object VisitFakeBlock(FakeBlockNode node) => null!;
}
