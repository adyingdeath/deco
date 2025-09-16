using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Types;

/// <summary>
/// Pass to perform type checking on the AST.
/// This pass traverses the AST and verifies type compatibility for assignments,
/// function call arguments, return statements, and expressions.
/// </summary>
public class TypeChecker(Scope globalSymbolTable) : IAstVisitor<Deco.Types.Type> {
    private readonly ScopeStack _scope = new(globalSymbolTable);
    private readonly Stack<FunctionNode> _functionStack = new();
    private readonly List<string> _errors = [];

    public List<string> Errors => _errors;

    public void Visit(AstNode astNode) {
        astNode.Accept(this);
    }

    public Deco.Types.Type VisitProgram(ProgramNode node) {
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

    public Deco.Types.Type VisitFunction(FunctionNode node) {
        _scope.PushScope(node.Scope);
        _functionStack.Push(node);

        node.Body.Accept(this);

        _scope.PopScope();
        _functionStack.Pop();
        return null!;
    }

    public Deco.Types.Type VisitBlock(BlockNode node) {
        _scope.PushScope(node.Scope);
        foreach (var stmt in node.Statements) {
            stmt.Accept(this);
        }
        _scope.PopScope();
        return null!;
    }

    public Deco.Types.Type VisitVariableDefinition(VariableDefinitionNode node) {
        // Check if the type annotation matches the initial value type
        var expectedType = TypeUtils.ParseType(node.Type);
        if (node.InitialValue != null) {
            var actualType = node.InitialValue.Accept(this);
            if (!AreTypesCompatible(actualType, expectedType)) {
                _errors.Add($"Type mismatch in variable definition '{node.Name}' at line {node.Line}: expected {expectedType}, got {actualType}");
            }
        }
        return null!;
    }

    public Deco.Types.Type VisitAssignment(AssignmentNode node) {
        var leftType = GetVariableType(node.Variable);
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

    public Deco.Types.Type VisitReturn(ReturnNode node) {
        if (_functionStack.Count == 0) {
            _errors.Add($"Return statement outside of function at line {node.Line}");
            return null!;
        }

        var currentFunction = _functionStack.Peek();
        var expectedReturnType = TypeUtils.ParseType(currentFunction.ReturnType);
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

    public Deco.Types.Type VisitIf(IfNode node) {
        var conditionType = node.Condition.Accept(this);
        var boolType = TypeUtils.BoolType;
        if (!AreTypesCompatible(conditionType, boolType)) {
            _errors.Add($"If condition must be bool type at line {node.Condition.Line}: got {conditionType}");
        }

        node.ThenBlock.Accept(this);
        node.ElseBlock?.Accept(this);
        return null!;
    }

    public Deco.Types.Type VisitWhile(WhileNode node) {
        var conditionType = node.Condition.Accept(this);
        var boolType = TypeUtils.BoolType;
        if (!AreTypesCompatible(conditionType, boolType)) {
            _errors.Add($"While condition must be bool type at line {node.Condition.Line}: got {conditionType}");
        }

        node.Body.Accept(this);
        return null!;
    }

    public Deco.Types.Type VisitFor(ForNode node) {
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

    public Deco.Types.Type VisitFunctionCall(FunctionCallNode node) {
        var symbol = _scope.Current().LookupSymbol(node.Name);
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

    public Deco.Types.Type VisitBinaryOp(BinaryOpNode node) {
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

    public Deco.Types.Type VisitUnaryOp(UnaryOpNode node) {
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

    public Deco.Types.Type VisitLiteral(LiteralNode node) {
        return node.Type switch {
            LiteralType.String => TypeUtils.StringType,
            LiteralType.Boolean => TypeUtils.BoolType,
            _ => TypeUtils.IntType, // Number and Null default to int
        };
    }

    public Deco.Types.Type VisitIdentifier(IdentifierNode node) {
        var symbol = _scope.Current().LookupSymbol(node.Name);
        if (symbol == null) {
            _errors.Add($"Undefined identifier '{node.Name}' at line {node.Line}");
            return TypeUtils.IntType; // Assume int to continue checking
        }
        return symbol.Type;
    }

    private Deco.Types.Type? GetVariableType(string name) {
        var symbol = _scope.Current().LookupSymbol(name);
        return symbol?.Type;
    }

    private static bool AreTypesCompatible(Deco.Types.Type actual, Deco.Types.Type expected) {
        return expected.Equals(actual);
    }

    private static bool IsNumericType(Deco.Types.Type type) {
        return type is PrimitiveType pt && (pt.Name == "int");
    }

    private static bool IsBoolType(Deco.Types.Type type) {
        return type is PrimitiveType pt && pt.Name == "bool";
    }

    private static bool IsStringType(Deco.Types.Type type) {
        return type is PrimitiveType pt && pt.Name == "string";
    }

    // Stub implementations for other nodes
    public Deco.Types.Type VisitModifier(ModifierNode node) => null!;
    public Deco.Types.Type VisitArgument(ArgumentNode node) => null!;
    public Deco.Types.Type VisitExpressionStatement(ExpressionStatementNode node) => node.Expression.Accept(this);
    public Deco.Types.Type VisitCommand(CommandNode node) => null!;
    public Deco.Types.Type VisitFakeBlock(FakeBlockNode node) => null!;
}
