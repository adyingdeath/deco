using Deco.Compiler.Diagnostics.Errors;
using Deco.Compiler.Types;

namespace Deco.Compiler.Ast.Passes;

/// <summary>
/// A type resolver pass that travels the AST, resolves types for all expressions,
/// and updates the Type properties of the nodes. Inherits from AstTransformVisitor
/// to transform and return updated nodes.
/// </summary>
public class TypeResolver(
    CompilationContext context, Scope globalSymbolTable
) : AstTransformVisitor {
    private readonly CompilationContext _context = context;
    private readonly ScopeStack _scope = new(globalSymbolTable);
    private readonly Stack<FunctionNode> _functionStack = new();

    /// <summary>
    /// Do the type checking and resolution step.
    /// Returns the AST with resolved types.
    /// </summary>
    public static AstNode Action(CompilationContext context, Scope symbolTable, AstNode astNode) {
        // Assume symbol tables are already built by Collect_Symbol passes
        var typeResolver = new TypeResolver(context, symbolTable);
        var resolvedAst = astNode.Accept(typeResolver);
        return resolvedAst;
    }

    public override AstNode VisitProgram(ProgramNode node) {
        // Set the global symbol table reference for the program node
        node.Scope = globalSymbolTable;

        // Pre-resolve all symbol types from UnresolvedType to actual types
        PreResolveSymbolTypes(globalSymbolTable);

        var newVarDefs = node.VariableDefinitions.Select(v => (VariableDefinitionNode)Visit(v)).ToList();
        var newFunctions = node.Functions.Select(f => (FunctionNode)Visit(f)).ToList();

        return new ProgramNode(
            newVarDefs, newFunctions, node.Line, node.Column
        ).CloneContext(node);
    }

    public override AstNode VisitFunction(FunctionNode node) {
        _scope.PushScope(node.Scope);
        _functionStack.Push(node);

        // Resolve its name's type
        var newName = (IdentifierNode)Visit(node.Name);
        var returnType = ResolveUnresolvedType(node.ReturnType);
        var newModifiers = node.Modifiers.Select(m => (ModifierNode)Visit(m)).ToList();
        var newArguments = node.Arguments.Select(a => (ArgumentNode)Visit(a)).ToList();
        var newBody = (BlockNode)Visit(node.Body);

        _functionStack.Pop();
        _scope.PopScope();

        return new FunctionNode(
            newModifiers, returnType, newName, newArguments, newBody,
            node.Line, node.Column
        ).CloneContext(node);
    }

    public override AstNode VisitBlock(BlockNode node) {
        _scope.PushScope(node.Scope);
        var newStatements = node.Statements.Select(s => (StatementNode)Visit(s)).ToList();
        _scope.PopScope();
        return new BlockNode(
            newStatements, node.Line, node.Column
        ).CloneContext(node);
    }

    public override AstNode VisitVariableDefinition(VariableDefinitionNode node) {
        /* First visit the name to potentially update its type. The VisitIdentifier
        will update the symbol's type first, then this IdentifierNode's type. */
        var newName = (IdentifierNode)Visit(node.Name);

        ExpressionNode? newInit = null;

        if (node.InitialValue != null) {
            newInit = (ExpressionNode)Visit(node.InitialValue);

            if (!newInit.Type.IsAssignableTo(newName.Type)) {
                _context.ErrorReporter.Report(new TypeMismatchError(
                    newName.Type, newInit.Type, newInit.Line, newInit.Column
                ));
            }
        }

        // Return new node with resolved identifier
        return node.With(name: newName, initialValue: newInit);
    }

    public override AstNode VisitAssignment(AssignmentNode node) {
        var newVariable = (IdentifierNode)Visit(node.Variable);
        var newExpression = (ExpressionNode)Visit(node.Expression);

        if (!newExpression.Type.IsAssignableTo(newVariable.Type)) {
            _context.ErrorReporter.Report(new TypeMismatchError(
                newVariable.Type, newExpression.Type, newExpression.Line, newExpression.Column
            ));
        }

        return node.With(newVariable, newExpression);
    }

    public override AstNode VisitReturn(ReturnNode node) {
        if (_functionStack.Count == 0) {
            _context.ErrorReporter.Report(
                new IllegalReturnStatementError(node.Line, node.Column)
            );
            return node;
        }

        FunctionNode currentFunction = _functionStack.Peek();
        IType expectedReturnType = currentFunction.ReturnType;
        if(expectedReturnType is UnresolvedType unresolved) {
            expectedReturnType = TypeUtils.ParseType(unresolved.Name);
        }

        ExpressionNode? newExpression = null;
        if (node.Expression != null) {
            newExpression = (ExpressionNode)Visit(node.Expression);
            if (!newExpression.Type.IsAssignableTo(expectedReturnType)) {
                _context.ErrorReporter.Report(new TypeMismatchError(
                    expectedReturnType, newExpression.Type, newExpression.Line, newExpression.Column
                ));
            }
        }
        
        return node.With(newExpression);
    }

    public override AstNode VisitIf(IfNode node) {
        var newCondition = (ExpressionNode)Visit(node.Condition);
        var newThenBlock = (BlockNode)Visit(node.ThenBlock);
        BlockNode? newElseBlock = null;

        if (node.ElseBlock != null) {
            newElseBlock = (BlockNode)Visit(node.ElseBlock);
        }

        // Check condition type
        if (!newCondition.Type.Equals(TypeUtils.BoolType)) {
            _context.ErrorReporter.Report(new TypeMismatchError(
                TypeUtils.BoolType, newCondition.Type, newCondition.Line, newCondition.Column
            ));
        }

        return node.With(newCondition, newThenBlock, newElseBlock);
    }

    public override AstNode VisitWhile(WhileNode node) {
        var newCondition = (ExpressionNode)Visit(node.Condition);
        var newBody = (BlockNode)Visit(node.Body);

        if (!newCondition.Type.Equals(TypeUtils.BoolType)) {
            _context.ErrorReporter.Report(new TypeMismatchError(
                TypeUtils.BoolType, newCondition.Type, newCondition.Line, newCondition.Column
            ));
        }

        return node.With(newCondition, newBody);
    }

    public override AstNode VisitFor(ForNode node) {
        _scope.PushScope(node.Scope);

        StatementNode? newInit = null;
        if (node.Initialization != null) {
            newInit = (StatementNode)Visit(node.Initialization);
        }

        ExpressionNode? newCondition = null;
        if (node.Condition != null) {
            newCondition = (ExpressionNode)Visit(node.Condition);
            if (!newCondition.Type.Equals(TypeUtils.BoolType)) {
                _context.ErrorReporter.Report(new TypeMismatchError(
                    TypeUtils.BoolType, newCondition.Type, newCondition.Line, newCondition.Column
                ));
            }
        }

        StatementNode? newIter = null;
        if (node.Iteration != null) {
            newIter = (StatementNode)Visit(node.Iteration);
        }

        var newBody = (BlockNode)Visit(node.Body);
        _scope.PopScope();

        return node.With(newInit, newCondition, newIter, newBody);
    }

    public override AstNode VisitBinaryOp(BinaryOpNode node) {
        var newLeft = (ExpressionNode)Visit(node.Left);
        var newRight = (ExpressionNode)Visit(node.Right);

        // Calculate and set the result type
        var resultType = CalculateBinaryOpType(
            newLeft.Type, node.Operator, newRight.Type,
            node.Line, node.Column
        );

        // Create new node and set its type
        return node.With(type: resultType, left: newLeft, right: newRight);
    }

    public override AstNode VisitUnaryOp(UnaryOpNode node) {
        var newOperand = (ExpressionNode)Visit(node.Operand);

        // Calculate and set the result type
        var resultType = CalculateUnaryOpType(node.Operator, newOperand.Type);

        // Create new node and set its type
        return node.With(type: resultType, operand: newOperand);
    }

    public override AstNode VisitFunctionCall(FunctionCallNode node) {
        var newName = (IdentifierNode)Visit(node.Name);
        var newArguments = node.Arguments.Select(a => (ExpressionNode)Visit(a)).ToList();

        // Resolve function type and set return type, defaults to void type.
        var symbol = _scope.Current().LookupSymbol(newName.Name);
        IType returnType = TypeUtils.VoidType;

        if (symbol?.Type is FunctionType funcType) {
            returnType = funcType.ReturnType;
            // Check argument types...
            if (newArguments.Count != funcType.ParameterTypes.Count) {
                _context.ErrorReporter.Report(new ArgumentCountMismatchError(
                    newName.Name,
                    funcType.ParameterTypes.Count,
                    newArguments.Count,
                    node.Line,
                    node.Column
                ));
            } else {
                for (int i = 0; i < newArguments.Count; i++) {
                    if (!newArguments[i].Type.IsAssignableTo(funcType.ParameterTypes[i])) {
                        _context.ErrorReporter.Report(new TypeMismatchError(
                            funcType.ParameterTypes[i], newArguments[i].Type, newArguments[i].Line, newArguments[i].Column
                        ));
                    }
                }
            }
        }

        // Create new node and set its type
        return node.With(type: returnType, name: newName, arguments: newArguments);
    }

    public override AstNode VisitIdentifier(IdentifierNode node) {
        var symbol = _scope.Current().LookupSymbol(node.Name);
        if (symbol == null) {
            _context.ErrorReporter.Report(new UndefinedIdentifierError(
                node.Name, node.Line, node.Column
            ));
            return node;
        }

        // Create new node and set its resolved type
        return node.With(type: symbol.Type, name: node.Name);
    }

    public override AstNode VisitLiteral(LiteralNode node) {
        // Literals already have correct types set, no change needed
        return node;
    }

    // Helper methods for type calculation
    private IType CalculateBinaryOpType(
        IType left, BinaryOperator op, IType right, int line, int column
    ) {
        switch (op) {
            case BinaryOperator.Add:
                if (
                    left.Equals(TypeUtils.StringType)
                    && right.Equals(TypeUtils.StringType)
                ) {
                    return TypeUtils.StringType;
                }
                // Fallthrough to handle numeric addition
                goto case BinaryOperator.Subtract;
            case BinaryOperator.Subtract:
            case BinaryOperator.Multiply:
            case BinaryOperator.Divide:
                if (left.IsNumeric || right.IsNumeric) {
                    // The result type is the one with the higher priority.
                    return left.PromotionPriority > right.PromotionPriority
                        ? left : right;
                }
                _context.ErrorReporter.Report(new NonNumericOperandError(
                    op, left, right, line, column
                ));
                return TypeUtils.UnknownType;

            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual:
                if (!(left.IsAssignableTo(right) || right.IsAssignableTo(left))
                    && !(left.IsNumeric && right.IsNumeric)
                ) {
                    _context.ErrorReporter.Report(
                        new IncompatibleComparisonOperandsError(
                            op, left, right, line, column
                        )
                    );
                }
                return TypeUtils.BoolType;

            case BinaryOperator.LessThan:
            case BinaryOperator.LessThanOrEqual:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.GreaterThanOrEqual:
                if (!left.IsNumeric || !right.IsNumeric) {
                    _context.ErrorReporter.Report(
                        new IncompatibleComparisonOperandsError(
                            op, left, right, line, column
                        )
                    );
                }
                return TypeUtils.BoolType;

            case BinaryOperator.LogicalAnd:
            case BinaryOperator.LogicalOr:
                IType? notBoolOne = null;
                if (!IsBoolType(left)) {
                    notBoolOne = left;
                } else if (!IsBoolType(right)) {
                    notBoolOne = right;
                }
                if (notBoolOne != null) {
                    _context.ErrorReporter.Report(new TypeMismatchError(
                        TypeUtils.BoolType, notBoolOne, line, column
                    ));
                    return TypeUtils.UnknownType;
                }
                return TypeUtils.BoolType;

            default:
                _context.ErrorReporter.Report(new UnknownBinaryOperatorError(line, column));
                return TypeUtils.UnknownType;
        }
    }

    private IType CalculateUnaryOpType(UnaryOperator op, IType operand) {
        return op switch {
            UnaryOperator.Negate => operand.IsNumeric ? operand : TypeUtils.UnknownType,
            UnaryOperator.LogicalNot => IsBoolType(operand) ? TypeUtils.BoolType : TypeUtils.UnknownType,
            _ => TypeUtils.UnknownType
        };
    }

    private static bool IsBoolType(IType type) {
        if (type is PrimitiveType pt) {
            return pt.Name == "bool";
        }
        return false;
    }

    /// <summary>
    /// Pre-resolves all symbol types from UnresolvedType to actual types in the symbol table.
    /// This ensures that when VisitIdentifier is called, symbol.Type is already resolved.
    /// </summary>
    private void PreResolveSymbolTypes(Scope scope) {
        foreach (var symbol in scope.Symbols.Values) {
            symbol.Type = ResolveUnresolvedType(symbol.Type);
        }

        // Recursively process child scopes
        foreach (var childScope in scope.Children) {
            PreResolveSymbolTypes(childScope);
        }
    }

    /// <summary>
    /// Recursively resolves an UnresolvedType by parsing it into its actual type.
    /// For FunctionTypes, resolves return type and parameter types.
    /// </summary>
    private IType ResolveUnresolvedType(IType type) {
        if (type is UnresolvedType unresolved) {
            return TypeUtils.ParseType(unresolved.Name);
        }

        if (type is FunctionType functionType) {
            var resolvedReturnType = ResolveUnresolvedType(functionType.ReturnType);
            var resolvedParameterTypes = functionType.ParameterTypes.Select(ResolveUnresolvedType).ToList();

            if (resolvedReturnType != functionType.ReturnType ||
                !resolvedParameterTypes.SequenceEqual(functionType.ParameterTypes)) {
                return new FunctionType(resolvedReturnType, resolvedParameterTypes);
            }
        }

        return type;
    }
    public override AstNode VisitArgument(ArgumentNode node) {
        var newName = (IdentifierNode)Visit(node.Name);
        return node.With(name: newName);
    }

    // Stub implementations for other nodes
    public override AstNode VisitModifier(ModifierNode node) => node;
    public override AstNode VisitExpressionStatement(ExpressionStatementNode node) =>
        new ExpressionStatementNode((ExpressionNode)Visit(node.Expression), node.Line, node.Column);
    public override AstNode VisitCommand(CommandNode node) => node;
    public override AstNode VisitFakeBlock(FakeBlockNode node) => node;
}
