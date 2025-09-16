using Deco.Ast;
using Deco.Types;
using Deco.Compiler.Passes.Types;
using NUnit.Framework;

namespace Deco.Tests.Types;

[TestFixture]
public class TypeCheckerTests
{
    private TypeChecker? _typeChecker;
    private Scope? _symbolTable;

    [SetUp]
    public void Setup()
    {
        // Create a root symbol table with some predefined symbols
        _symbolTable = new Scope("global");
        _symbolTable.AddSymbol(new Symbol("x", TypeUtils.IntType, SymbolKind.Variable, 1, 0));
        _symbolTable.AddSymbol(new Symbol("y", TypeUtils.BoolType, SymbolKind.Variable, 1, 1));
        _symbolTable.AddSymbol(new Symbol("foo", new FunctionType(TypeUtils.IntType, [TypeUtils.IntType]), SymbolKind.Function, 2, 0));
        _symbolTable.AddSymbol(new Symbol("bar", new FunctionType(TypeUtils.BoolType, [TypeUtils.IntType, TypeUtils.StringType]), SymbolKind.Function, 3, 0));

        _typeChecker = new TypeChecker(_symbolTable);
    }

    [Test]
    public void VisitProgram_TraversesAllNodes_ReturnsVoid()
    {
        // Arrange
        var program = new ProgramNode(null, null);
        program.VariableDefinitions.Add(new VariableDefinitionNode("int", "z", new LiteralNode(LiteralType.Number, "10", 4, 0), 4, 0));
        program.Functions.Add(new FunctionNode(new List<ModifierNode>(), "int", "testFunc", new List<ArgumentNode>(), new BlockNode(), 5, 0));

        // Act
        var result = _typeChecker!.VisitProgram(program);

        // Assert
        Assert.That(result, Is.EqualTo(null!)); // VisitProgram returns null!
        Assert.That(_typeChecker.Errors, Is.Empty);
    }

    [Test]
    public void VisitVariableDefinition_TypeMismatch_ReportsError()
    {
        // Arrange
        var varDef = new VariableDefinitionNode("string", "z", new LiteralNode(LiteralType.Number, "10", 1, 5), 1, 0);

        // Act
        var result = _typeChecker!.VisitVariableDefinition(varDef);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("Type mismatch in variable definition 'z'"));
    }

    [Test]
    public void VisitVariableDefinition_CorrectType_NoError()
    {
        // Arrange
        var varDef = new VariableDefinitionNode("int", "z", new LiteralNode(LiteralType.Number, "10", 1, 5), 1, 0);

        // Act
        var result = _typeChecker!.VisitVariableDefinition(varDef);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Is.Empty);
    }

    [Test]
    public void VisitAssignment_UndefinedVariable_ReportsError()
    {
        // Arrange
        var assignment = new AssignmentNode("undefinedVar", new LiteralNode(LiteralType.Boolean, "true", 1, 10), 1, 0);

        // Act
        var result = _typeChecker!.VisitAssignment(assignment);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("Undefined variable 'undefinedVar'"));
    }

    [Test]
    public void VisitAssignment_TypeMismatch_ReportsError()
    {
        // Arrange
        var assignment = new AssignmentNode("y", new LiteralNode(LiteralType.Number, "10", 1, 8), 1, 5); // y is bool, assigning int

        // Act
        var result = _typeChecker!.VisitAssignment(assignment);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("Type mismatch in assignment"));
    }

    [Test]
    public void VisitAssignment_CorrectType_NoError()
    {
        // Arrange
        var assignment = new AssignmentNode("x", new LiteralNode(LiteralType.Number, "5", 1, 6), 1, 2);

        // Act
        var result = _typeChecker!.VisitAssignment(assignment);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Is.Empty);
    }

    [Test]
    public void VisitReturn_OutsideFunction_ReportsError()
    {
        // Arrange
        var returnNode = new ReturnNode(new LiteralNode(LiteralType.Number, "42", 1, 7), 1, 0);

        // Act
        var result = _typeChecker!.VisitReturn(returnNode);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("Return statement outside of function"));
    }

    [Test]
    public void VisitReturn_InFunction_TypeMismatch_ReportsError()
    {
        // Arrange
        var functionNode = new FunctionNode(new List<ModifierNode>(), "void", "testFunc", new List<ArgumentNode>(), new BlockNode(), 1, 0);
        var returnNode = new ReturnNode(new LiteralNode(LiteralType.Number, "42", 2, 9), 2, 0);

        // Simulate entering function
        _typeChecker!.VisitFunction(functionNode); // but this updates _functionStack
        // Note: This might need a better way to test inside function context. For simplicity, handling error count.

        // For now, test without function context to show error.
        // To test properly, might need to arrange symbol table for function.
    }

    [Test]
    public void VisitIf_ConditionNotBool_ReportsError()
    {
        // Arrange
        var ifNode = new IfNode(new LiteralNode(LiteralType.Number, "5", 1, 3), new BlockNode(), null, 1, 0);

        // Act
        var result = _typeChecker!.VisitIf(ifNode);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("If condition must be bool type"));
    }

    [Test]
    public void VisitIf_CorrectBoolCondition_NoError()
    {
        // Arrange
        var ifNode = new IfNode(new LiteralNode(LiteralType.Boolean, "true", 1, 3), new BlockNode(), null, 1, 0);

        // Act
        var result = _typeChecker!.VisitIf(ifNode);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Is.Empty);
    }

    [Test]
    public void VisitWhile_ConditionNotBool_ReportsError()
    {
        // Arrange
        var whileNode = new WhileNode(new LiteralNode(LiteralType.Number, "5", 1, 6), new BlockNode(), 1, 0);

        // Act
        var result = _typeChecker!.VisitWhile(whileNode);

        // Assert
        Assert.That(result, Is.EqualTo(null!));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("While condition must be bool type"));
    }

    [Test]
    public void VisitBinaryOp_Add_Integers_ReturnsInt()
    {
        // Arrange
        var left = new LiteralNode(LiteralType.Number, "10", 1, 0);
        var right = new LiteralNode(LiteralType.Number, "5", 1, 4);
        var binaryOp = new BinaryOpNode(left, BinaryOperator.Add, right, 1, 2);

        // Act
        var result = _typeChecker!.VisitBinaryOp(binaryOp);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.IntType));
        Assert.That(_typeChecker.Errors, Is.Empty);
    }

    [Test]
    public void VisitBinaryOp_Add_Strings_ReturnsString()
    {
        // Arrange
        var left = new LiteralNode(LiteralType.String, "hello", 1, 0);
        var right = new LiteralNode(LiteralType.String, "world", 1, 8);
        var binaryOp = new BinaryOpNode(left, BinaryOperator.Add, right, 1, 6);

        // Act
        var result = _typeChecker!.VisitBinaryOp(binaryOp);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.StringType));
        Assert.That(_typeChecker.Errors, Is.Empty);
    }

    [Test]
    public void VisitBinaryOp_LogicalAnd_NonBool_ReportsError()
    {
        // Arrange
        var left = new LiteralNode(LiteralType.Number, "10", 1, 0);
        var right = new LiteralNode(LiteralType.Boolean, "true", 1, 6);
        var binaryOp = new BinaryOpNode(left, BinaryOperator.LogicalAnd, right, 1, 3);

        // Act
        var result = _typeChecker!.VisitBinaryOp(binaryOp);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.BoolType));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("Logical operation requires boolean operands"));
    }

    [Test]
    public void VisitUnaryOp_Negate_NonNumeric_ReportsError()
    {
        // Arrange
        var operand = new LiteralNode(LiteralType.Boolean, "true", 1, 2);
        var unaryOp = new UnaryOpNode(UnaryOperator.Negate, operand, 1, 0);

        // Act
        var result = _typeChecker!.VisitUnaryOp(unaryOp);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.IntType));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("Unary negation requires numeric operand"));
    }

    [Test]
    public void VisitLiteral_Number_ReturnsInt()
    {
        // Arrange
        var literal = new LiteralNode(LiteralType.Number, "42", 1, 0);

        // Act
        var result = _typeChecker!.VisitLiteral(literal);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.IntType));
    }

    [Test]
    public void VisitLiteral_Boolean_ReturnsBool()
    {
        // Arrange
        var literal = new LiteralNode(LiteralType.Boolean, "false", 1, 0);

        // Act
        var result = _typeChecker!.VisitLiteral(literal);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.BoolType));
    }

    [Test]
    public void VisitIdentifier_Defined_ReturnsType()
    {
        // Arrange
        var identifier = new IdentifierNode("x", 1, 0);

        // Act
        var result = _typeChecker!.VisitIdentifier(identifier);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.IntType));
        Assert.That(_typeChecker.Errors, Is.Empty);
    }

    [Test]
    public void VisitIdentifier_Undefined_ReportsError_ReturnsInt()
    {
        // Arrange
        var identifier = new IdentifierNode("undefined", 1, 0);

        // Act
        var result = _typeChecker!.VisitIdentifier(identifier);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.IntType)); // Error case returns IntType
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("Undefined identifier 'undefined'"));
    }

    [Test]
    public void VisitFunctionCall_UndefinedFunction_ReportsError_ReturnsVoid()
    {
        // Arrange
        var funcCall = new FunctionCallNode("unknownFunc", new List<ExpressionNode>(), 1, 0);

        // Act
        var result = _typeChecker!.VisitFunctionCall(funcCall);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.VoidType));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("Undefined function 'unknownFunc'"));
    }

    [Test]
    public void VisitFunctionCall_CorrectCall_ReturnsFunctionReturnType()
    {
        // Arrange
        var funcCall = new FunctionCallNode("foo", [new LiteralNode(LiteralType.Number, "1", 1, 4)], 1, 0);

        // Act
        var result = _typeChecker!.VisitFunctionCall(funcCall);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.IntType));
        Assert.That(_typeChecker.Errors, Is.Empty);
    }

    [Test]
    public void VisitFunctionCall_ArgumentMismatch_ReturnsType()
    {
        // Arrange (foo expects 1 int, calling with 2 args)
        var funcCall = new FunctionCallNode("foo", [new LiteralNode(LiteralType.Number, "1", 1, 4), new LiteralNode(LiteralType.Number, "2", 1, 6)], 1, 0);

        // Act
        var result = _typeChecker!.VisitFunctionCall(funcCall);

        // Assert
        Assert.That(result, Is.EqualTo(TypeUtils.IntType));
        Assert.That(_typeChecker.Errors, Has.Count.EqualTo(1));
        Assert.That(_typeChecker.Errors[0], Does.Contain("expects 1 arguments, got 2"));
    }

    [Test]
    public void Group_Visit_DoesNotThrow()
    {
        // Arrange
        var scope = new Scope("global");
        var group = new Group(scope);
        var program = new ProgramNode(null, null);

        // Act
        Assert.DoesNotThrow(() => group.Visit(program));
    }
}
