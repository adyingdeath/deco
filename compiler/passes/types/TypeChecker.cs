using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Types;

/// <summary>
/// Pass to perform type checking on the AST.
/// This pass traverses the AST and verifies type compatibility for assignments,
/// function call arguments, return statements, and expressions.
/// </summary>
public class TypeChecker(Scope globalSymbolTable) : IAstVisitor<Deco.Types.IType> {
    private readonly ScopeStack _scope = new(globalSymbolTable);
    private readonly Stack<FunctionNode> _functionStack = new();
    private readonly List<string> _errors = [];

    public List<string> Errors => _errors;

    public void Visit(AstNode astNode) {
        astNode.Accept(this);
    }

    public IType VisitProgram(ProgramNode node) {
        // Visit global variable definitions
        foreach (var varDef in node.VariableDefinitions) {
            varDef.Accept(this);
        }

        // Visit functions
        foreach (var func in node.Functions) {
            func.Accept(this);
        }

        return null!;
    }

    public IType VisitFunction(FunctionNode node) {
        _scope.PushScope(node.Scope);
        _functionStack.Push(node);

        node.Body.Accept(this);

        _scope.PopScope();
        _functionStack.Pop();
        return null!;
    }

    public IType VisitBlock(BlockNode node) {
        _scope.PushScope(node.Scope);
        foreach (var stmt in node.Statements) {
            stmt.Accept(this);
        }
        _scope.PopScope();
        return null!;
    }

    public IType VisitVariableDefinition(VariableDefinitionNode node) {
        var expectedType = node.Type;
        // Check if the type annotation matches the initial value type
        if (node.InitialValue != null) {
            var actualType = node.InitialValue.Accept(this);
            if (!AreTypesCompatible(actualType, expectedType)) {
                _errors.Add($"Type mismatch in variable definition '{node.Name}' at line {node.Line}: expected {expectedType}, got {actualType}");
            }
        }
        return null!;
    }

    public IType VisitAssignment(AssignmentNode node) {
        var leftType = node.Variable.Type;
        if (leftType == null) {
            _errors.Add($"Undefined variable '{node.Variable}' in assignment at line {node.Line}");
            return null!;
        }

        var rightType = node.Expression.Accept(this);
        if (!AreTypesCompatible(rightType, leftType)) {
            _errors.Add($"Type mismatch in assignment to '{node.Variable}' at line {node.Line}: expected {leftType}, got {rightType}");
        }
        return null!;
    }

    public IType VisitReturn(ReturnNode node) {
        if (_functionStack.Count == 0) {
            _errors.Add($"Return statement outside of function at line {node.Line}");
            return null!;
        }

        var currentFunction = _functionStack.Peek();
        var expectedReturnType = currentFunction.ReturnType;
        if (node.Expression == null) {
            // Return without expression (should be void)
            if (!(expectedReturnType is PrimitiveType pt && pt.Name == "void")) {
                _errors.Add($"Empty return in non-void function '{currentFunction.Name}' at line {node.Line}");
            }
        } else {
            var actualType = node.Expression.Accept(this);
            if (!AreTypesCompatible(actualType, expectedReturnType)) {
                _errors.Add($"Return type mismatch in function '{currentFunction.Name}' at line {node.Line}: expected {expectedReturnType}, got {actualType}");
            }
        }
        return null!;
    }

    public IType VisitIf(IfNode node) {
        var conditionType = node.Condition.Accept(this);
        var boolType = TypeUtils.BoolType;
        if (!AreTypesCompatible(conditionType, boolType)) {
            _errors.Add($"If condition must be bool type at line {node.Condition.Line}: got {conditionType}");
        }

        node.ThenBlock.Accept(this);
        node.ElseBlock?.Accept(this);
        return null!;
    }

    public IType VisitWhile(WhileNode node) {
        var conditionType = node.Condition.Accept(this);
        var boolType = TypeUtils.BoolType;
        if (!AreTypesCompatible(conditionType, boolType)) {
            _errors.Add($"While condition must be bool type at line {node.Condition.Line}: got {conditionType}");
        }

        node.Body.Accept(this);
        return null!;
    }

    public IType VisitFor(ForNode node) {
        _scope.PushScope(node.Scope);

        if (node.Initialization != null) {
            node.Initialization.Accept(this);
        }

        if (node.Condition != null) {
            var conditionType = node.Condition.Accept(this);
            var boolType = TypeUtils.BoolType;
            if (!AreTypesCompatible(conditionType, boolType)) {
                _errors.Add($"For loop condition must be bool type at line {node.Condition.Line}: got {conditionType}");
            }
        }

        if (node.Iteration != null) {
            node.Iteration.Accept(this);
        }

        node.Body.Accept(this);
        _scope.PopScope();
        return null!;
    }

    public IType VisitFunctionCall(FunctionCallNode node) {
        var symbol = _scope.Current().LookupSymbol(node.Name.Name);
        if (symbol == null) {
            _errors.Add($"Undefined function '{node.Name}' at line {node.Line}");
            return TypeUtils.VoidType; // Return void to avoid further errors
        }

        if (symbol.Type is not FunctionType funcType) {
            _errors.Add($"'{node.Name}' is not a function at line {node.Line}");
            return TypeUtils.VoidType;
        }

        if (node.Arguments.Count != funcType.ParameterTypes.Count) {
            _errors.Add($"Function '{node.Name}' expects {funcType.ParameterTypes.Count} arguments, got {node.Arguments.Count} at line {node.Line}");
            return funcType.ReturnType;
        }

        for (int i = 0; i < node.Arguments.Count; i++) {
            var argType = node.Arguments[i].Accept(this);
            var paramType = funcType.ParameterTypes[i];
            if (!AreTypesCompatible(argType, paramType)) {
                _errors.Add($"Argument {i + 1} of function '{node.Name}' type mismatch at line {node.Arguments[i].Line}: expected {paramType}, got {argType}");
            }
        }

        return funcType.ReturnType;
    }

    public IType VisitBinaryOp(BinaryOpNode node) {
        var leftType = node.Left.Accept(this);
        var rightType = node.Right.Accept(this);

        // For now, check simple cases: numeric operations, boolean operations, etc.
        switch (node.Operator) {
            case BinaryOperator.Add:
                if (IsStringType(leftType) && IsStringType(rightType)) {
                    return TypeUtils.StringType; // String concatenation
                }
                if (IsNumericType(leftType) && IsNumericType(rightType)) {
                    return TypeUtils.IntType;
                }
                _errors.Add($"'+' operation requires numeric or string operands at line {node.Line}: got {leftType} and {rightType}");
                return TypeUtils.IntType;

            case BinaryOperator.Subtract:
            case BinaryOperator.Multiply:
            case BinaryOperator.Divide:
                if (!IsNumericType(leftType) || !IsNumericType(rightType)) {
                    _errors.Add($"Arithmetic operation requires numeric operands at line {node.Line}: got {leftType} and {rightType}");
                }
                return TypeUtils.IntType;

            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual:
                // Allow comparison between same types or numeric types
                if (!AreTypesCompatible(leftType, rightType) && !(IsNumericType(leftType) && IsNumericType(rightType))) {
                    _errors.Add($"Comparison operation requires compatible operands at line {node.Line}: got {leftType} and {rightType}");
                }
                return TypeUtils.BoolType;

            case BinaryOperator.LessThan:
            case BinaryOperator.LessThanOrEqual:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.GreaterThanOrEqual:
                if (!IsNumericType(leftType) || !IsNumericType(rightType)) {
                    _errors.Add($"Ordering comparison requires numeric operands at line {node.Line}: got {leftType} and {rightType}");
                }
                return TypeUtils.BoolType;

            case BinaryOperator.LogicalAnd:
            case BinaryOperator.LogicalOr:
                if (!IsBoolType(leftType) || !IsBoolType(rightType)) {
                    _errors.Add($"Logical operation requires boolean operands at line {node.Line}: got {leftType} and {rightType}");
                }
                return TypeUtils.BoolType;

            default:
                _errors.Add($"Unknown binary operator '{node.Operator}' at line {node.Line}");
                return TypeUtils.IntType;
        }
    }

    public IType VisitUnaryOp(UnaryOpNode node) {
        var operandType = node.Operand.Accept(this);

        switch (node.Operator) {
            case UnaryOperator.Negate:
                if (!IsNumericType(operandType)) {
                    _errors.Add($"Unary negation requires numeric operand at line {node.Line}: got {operandType}");
                    return TypeUtils.IntType;
                }
                return TypeUtils.IntType;

            case UnaryOperator.LogicalNot:
                if (!IsBoolType(operandType)) {
                    _errors.Add($"Unary logical not requires boolean operand at line {node.Line}: got {operandType}");
                    return TypeUtils.BoolType;
                }
                return TypeUtils.BoolType;

            default:
                _errors.Add($"Unknown unary operator '{node.Operator}' at line {node.Line}");
                return TypeUtils.IntType;
        }
    }

    public IType VisitLiteral(LiteralNode node) {
        return node.Type;
    }

    public IType VisitIdentifier(IdentifierNode node) {
        var symbol = _scope.Current().LookupSymbol(node.Name);
        if (symbol == null) {
            _errors.Add($"Undefined identifier '{node.Name}' at line {node.Line}");
            return TypeUtils.IntType; // Assume int to continue checking
        }
        return symbol.Type;
    }

    private Deco.Types.IType? GetVariableType(string name) {
        var symbol = _scope.Current().LookupSymbol(name);
        return symbol?.Type;
    }

    private static bool AreTypesCompatible(Deco.Types.IType actual, Deco.Types.IType expected) {
        return expected.Equals(actual);
    }

    private static bool IsNumericType(Deco.Types.IType type) {
        return type is PrimitiveType pt && (pt.Name == "int");
    }

    private static bool IsBoolType(Deco.Types.IType type) {
        return type is PrimitiveType pt && pt.Name == "bool";
    }

    private static bool IsStringType(Deco.Types.IType type) {
        return type is PrimitiveType pt && pt.Name == "string";
    }

    // Stub implementations for other nodes
    public IType VisitModifier(ModifierNode node) => null!;
    public IType VisitArgument(ArgumentNode node) => null!;
    public IType VisitExpressionStatement(ExpressionStatementNode node) => node.Expression.Accept(this);
    public IType VisitCommand(CommandNode node) => null!;
    public IType VisitFakeBlock(FakeBlockNode node) => null!;
}
