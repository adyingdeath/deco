using Antlr4.Runtime;
using Deco.Ast;
using NUnit.Framework;

namespace Deco.Tests.Ast;

[TestFixture]
public class AstBuilderTests {
    private AstBuilder _astBuilder = null!;

    [SetUp]
    public void Setup() {
        _astBuilder = new AstBuilder();
    }

    private ProgramNode BuildAst(string code) {
        var stream = CharStreams.fromString(code);
        var lexer = new DecoLexer(stream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new DecoParser(tokens);
        var tree = parser.program();
        return (ProgramNode)_astBuilder.Visit(tree);
    }

    private FunctionNode GetFirstFunction(string code) {
        var program = BuildAst(code);
        Assert.That(program.Functions, Is.Not.Empty, "Expected at least one function to be parsed.");
        return program.Functions[0];
    }

    private StatementNode GetFirstStatement(string functionBodyCode) {
        var function = GetFirstFunction($"void test() {{ {functionBodyCode} }}");
        Assert.That(function.Body.Statements, Is.Not.Empty, "Expected at least one statement in the function body.");
        return function.Body.Statements[0];
    }

    #region Program and Function Tests

    [Test]
    public void VisitProgram_EmptyCode_ReturnsEmptyProgramNode() {
        var program = BuildAst("");
        Assert.That(program.Functions, Is.Empty);
    }

    [Test]
    public void VisitFunction_BasicFunction_ParsesCorrectly() {
        var function = GetFirstFunction("void main() {}");
        Assert.Multiple(() => {
            Assert.That(function.Name, Is.EqualTo("main"));
            Assert.That(function.ReturnType, Is.EqualTo("void"));
            Assert.That(function.Modifiers, Is.Empty);
            Assert.That(function.Arguments, Is.Empty);
            Assert.That(function.Body.Statements, Is.Empty);
        });
    }

    [Test]
    public void VisitFunction_WithArguments_ParsesArguments() {
        var function = GetFirstFunction("int add(int a, string b) {}");
        Assert.Multiple(() => {
            Assert.That(function.Name, Is.EqualTo("add"));
            Assert.That(function.ReturnType, Is.EqualTo("int"));
            Assert.That(function.Arguments, Has.Count.EqualTo(2));
            Assert.That(function.Arguments[0].Name, Is.EqualTo("a"));
            Assert.That(function.Arguments[0].Type, Is.EqualTo("int"));
            Assert.That(function.Arguments[1].Name, Is.EqualTo("b"));
            Assert.That(function.Arguments[1].Type, Is.EqualTo("string"));
        });
    }

    [Test]
    public void VisitFunction_WithModifiers_ParsesModifiers() {
        var function = GetFirstFunction(@"
        @load
        @tick
        void main() {

        }
        ");
        
        Assert.Multiple(() => {
            Assert.That(function.Modifiers, Has.Count.EqualTo(2));
            Assert.That(function.Modifiers[0].Name, Is.EqualTo("load"));
            Assert.That(function.Modifiers[1].Name, Is.EqualTo("tick"));
        });
    }

    [Test]
    public void VisitFunction_WithModifierParameters_ParsesModifierParameters() {
        var function = GetFirstFunction(@"
        @schedule(10, ""ticks"")
        void repeating_task() {}
        ");
        
        Assert.That(function.Modifiers, Has.Count.EqualTo(1));
        var modifier = function.Modifiers[0];
        Assert.Multiple(() => {
            Assert.That(modifier.Name, Is.EqualTo("schedule"));
            Assert.That(modifier.Parameters, Has.Count.EqualTo(2));
        });

        var param1 = modifier.Parameters[0] as LiteralNode;
        var param2 = modifier.Parameters[1] as LiteralNode;
        Assert.Multiple(() => {
            Assert.That(param1, Is.Not.Null);
            Assert.That(param1?.Type, Is.EqualTo(LiteralType.Number));
            Assert.That(param1?.Value, Is.EqualTo("10"));
            Assert.That(param2, Is.Not.Null);
            Assert.That(param2?.Type, Is.EqualTo(LiteralType.String));
            Assert.That(param2?.Value, Is.EqualTo("ticks"));
        });
    }

    #endregion

    #region Statement Tests

    [Test]
    public void VisitStatement_CommandStatement_ParsesCorrectly() {
        var statement = GetFirstStatement("@`say Hello World`;");
        Assert.That(statement, Is.InstanceOf<CommandNode>());
        var commandNode = (CommandNode)statement;
        Assert.That(commandNode.Command, Is.EqualTo("@`say Hello World`"));
    }

    [Test]
    public void VisitStatement_VariableDefinition_ParsesCorrectly() {
        var statement = GetFirstStatement("int my_score;");
        Assert.That(statement, Is.InstanceOf<VariableDefinitionNode>());
        var varDefNode = (VariableDefinitionNode)statement;
        Assert.Multiple(() => {
            Assert.That(varDefNode.Type, Is.EqualTo("int"));
            Assert.That(varDefNode.Name, Is.EqualTo("my_score"));
        });
    }

    [Test]
    public void VisitStatement_Assignment_ParsesCorrectly() {
        var statement = GetFirstStatement("my_var = 123 + 456;");
        Assert.That(statement, Is.InstanceOf<AssignmentNode>());
        var assignmentNode = (AssignmentNode)statement;
        Assert.Multiple(() => {
            Assert.That(assignmentNode.Variable, Is.EqualTo("my_var"));
            Assert.That(assignmentNode.Expression, Is.InstanceOf<BinaryOpNode>());
        });
    }

    [Test]
    public void VisitStatement_ReturnStatementWithValue_ParsesCorrectly() {
        var statement = GetFirstStatement("return 0;");
        Assert.That(statement, Is.InstanceOf<ReturnNode>());
        var returnNode = (ReturnNode)statement;
        Assert.That(returnNode.Expression, Is.InstanceOf<LiteralNode>());
    }

    [Test]
    public void VisitStatement_ReturnStatementWithoutValue_ParsesCorrectly() {
        var statement = GetFirstStatement("return;");
        Assert.That(statement, Is.InstanceOf<ReturnNode>());
        var returnNode = (ReturnNode)statement;
        Assert.That(returnNode.Expression, Is.Null);
    }

    #endregion

    #region Control Flow Tests

    [Test]
    public void VisitIfStatement_SimpleIf_ParsesCorrectly() {
        var statement = GetFirstStatement("if (true) { int a; }");
        Assert.That(statement, Is.InstanceOf<IfNode>());
        var ifNode = (IfNode)statement;
        Assert.Multiple(() => {
            Assert.That(ifNode.Condition, Is.InstanceOf<LiteralNode>());
            Assert.That(ifNode.ThenBlock.Statements, Has.Count.EqualTo(1));
            Assert.That(ifNode.ElseBlock, Is.Null);
        });
    }

    [Test]
    public void VisitIfStatement_IfElse_ParsesCorrectly() {
        var statement = GetFirstStatement("if (x > 0) {} else { x = 0; }");
        Assert.That(statement, Is.InstanceOf<IfNode>());
        var ifNode = (IfNode)statement;
        Assert.Multiple(() => {
            Assert.That(ifNode.Condition, Is.InstanceOf<BinaryOpNode>());
            Assert.That(ifNode.ThenBlock.Statements, Is.Empty);
            Assert.That(ifNode.ElseBlock, Is.Not.Null);
            Assert.That(ifNode.ElseBlock?.Statements, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void VisitIfStatement_IfElseIfElse_ParsesCorrectly() {
        var statement = GetFirstStatement("if (a == 1) {} else if (a == 2) {} else {}");
        Assert.That(statement, Is.InstanceOf<IfNode>());

        var rootIf = (IfNode)statement;
        Assert.Multiple(() => {
            Assert.That(rootIf.Condition, Is.InstanceOf<BinaryOpNode>());
            Assert.That(((BinaryOpNode)rootIf.Condition).Right, Is.InstanceOf<LiteralNode>());
            Assert.That(rootIf.ThenBlock.Statements, Is.Empty);
            Assert.That(rootIf.ElseBlock, Is.Not.Null);
        });
        
        // The else block contains the 'else if' statement
        Assert.That(rootIf.ElseBlock!.Statements, Has.Count.EqualTo(1));
        Assert.That(rootIf.ElseBlock.Statements[0], Is.InstanceOf<IfNode>());
        
        var elseIfNode = (IfNode)rootIf.ElseBlock.Statements[0];
        Assert.Multiple(() => {
            Assert.That(elseIfNode.Condition, Is.InstanceOf<BinaryOpNode>());
            var rightOfElseIf = ((BinaryOpNode)elseIfNode.Condition).Right as LiteralNode;
            Assert.That(rightOfElseIf?.Value, Is.EqualTo("2"));
            Assert.That(elseIfNode.ThenBlock.Statements, Is.Empty);
            Assert.That(elseIfNode.ElseBlock, Is.Not.Null);
            Assert.That(elseIfNode.ElseBlock?.Statements, Is.Empty);
        });
    }

    [Test]
    public void VisitWhileStatement_ParsesCorrectly() {
        var statement = GetFirstStatement("while (i < 10) { i = i + 1; }");
        Assert.That(statement, Is.InstanceOf<WhileNode>());
        var whileNode = (WhileNode)statement;
        Assert.Multiple(() => {
            Assert.That(whileNode.Condition, Is.InstanceOf<BinaryOpNode>());
            Assert.That(whileNode.Body.Statements, Has.Count.EqualTo(1));
        });
    }

    #endregion

    #region Expression Tests

    [Test]
    public void VisitExpression_OperatorPrecedence_MulBeforeAdd() {
        var statement = GetFirstStatement("x = a + b * c;");
        var assignment = (AssignmentNode)statement;
        var expr = (BinaryOpNode)assignment.Expression;

        // AST should be: (a + (b * c))
        Assert.Multiple(() => {
            Assert.That(expr.Operator, Is.EqualTo(BinaryOperator.Add));
            Assert.That(expr.Left, Is.InstanceOf<IdentifierNode>());
            Assert.That(expr.Right, Is.InstanceOf<BinaryOpNode>());
        });

        var rightSubExpr = (BinaryOpNode)expr.Right;
        Assert.That(rightSubExpr.Operator, Is.EqualTo(BinaryOperator.Multiply));
    }

    [Test]
    public void VisitExpression_Parentheses_OverridesPrecedence() {
        var statement = GetFirstStatement("x = (a + b) * c;");
        var assignment = (AssignmentNode)statement;
        var expr = (BinaryOpNode)assignment.Expression;

        // AST should be: ((a + b) * c)
        Assert.Multiple(() => {
            Assert.That(expr.Operator, Is.EqualTo(BinaryOperator.Multiply));
            Assert.That(expr.Left, Is.InstanceOf<BinaryOpNode>());
            Assert.That(expr.Right, Is.InstanceOf<IdentifierNode>());
        });

        var leftSubExpr = (BinaryOpNode)expr.Left;
        Assert.That(leftSubExpr.Operator, Is.EqualTo(BinaryOperator.Add));
    }

    [Test]
    public void VisitExpression_LeftAssociativity_ChainedSubtraction() {
        var statement = GetFirstStatement("x = a - b - c;");
        var assignment = (AssignmentNode)statement;
        var expr = (BinaryOpNode)assignment.Expression;

        // AST should be: ((a - b) - c)
        Assert.Multiple(() => {
            Assert.That(expr.Operator, Is.EqualTo(BinaryOperator.Subtract));
            Assert.That(expr.Left, Is.InstanceOf<BinaryOpNode>());
            Assert.That(expr.Right, Is.InstanceOf<IdentifierNode>());
        });
        
        var rightIdentifier = (IdentifierNode)expr.Right;
        Assert.That(rightIdentifier.Name, Is.EqualTo("c"));

        var leftSubExpr = (BinaryOpNode)expr.Left;
        Assert.Multiple(() => {
            Assert.That(leftSubExpr.Operator, Is.EqualTo(BinaryOperator.Subtract));
            Assert.That(leftSubExpr.Left, Is.InstanceOf<IdentifierNode>());
            Assert.That(leftSubExpr.Right, Is.InstanceOf<IdentifierNode>());
        });
    }

    [Test]
    public void VisitExpression_UnaryOperators_ParsesCorrectly() {
        var statement = GetFirstStatement("x = -5 + !is_active;");
        var expr = ((AssignmentNode)statement).Expression as BinaryOpNode;
        
        Assert.That(expr, Is.Not.Null);
        Assert.Multiple(() => {
            Assert.That(expr.Left, Is.InstanceOf<UnaryOpNode>());
            Assert.That(expr.Right, Is.InstanceOf<UnaryOpNode>());
        });
        
        var leftUnary = (UnaryOpNode)expr.Left;
        Assert.Multiple(() => {
            Assert.That(leftUnary.Operator, Is.EqualTo(UnaryOperator.Negate));
            Assert.That(leftUnary.Operand, Is.InstanceOf<LiteralNode>());
        });

        var rightUnary = (UnaryOpNode)expr.Right;
        Assert.Multiple(() => {
            Assert.That(rightUnary.Operator, Is.EqualTo(UnaryOperator.LogicalNot));
            Assert.That(rightUnary.Operand, Is.InstanceOf<IdentifierNode>());
        });
    }

    [Test]
    public void VisitExpression_FunctionCall_ParsesCorrectly() {
        var statement = GetFirstStatement("some_func(1, \"hello\", c);");
        var exprStatement = (ExpressionStatementNode)statement;
        var funcCall = (FunctionCallNode)exprStatement.Expression;
        
        Assert.Multiple(() => {
            Assert.That(funcCall.Name, Is.EqualTo("some_func"));
            Assert.That(funcCall.Arguments, Has.Count.EqualTo(3));
            Assert.That(funcCall.Arguments[0], Is.InstanceOf<LiteralNode>());
            Assert.That(funcCall.Arguments[1], Is.InstanceOf<LiteralNode>());
            Assert.That(funcCall.Arguments[2], Is.InstanceOf<IdentifierNode>());
        });
    }
    
    [Test]
    public void VisitExpression_NestedFunctionCalls_ParsesCorrectly() {
        var statement = GetFirstStatement("func_a(func_b(1), func_c());");
        var funcCallA = ((ExpressionStatementNode)statement).Expression as FunctionCallNode;
        
        Assert.That(funcCallA, Is.Not.Null);
        Assert.That(funcCallA.Arguments, Has.Count.EqualTo(2));
        
        var funcCallB = funcCallA.Arguments[0] as FunctionCallNode;
        Assert.That(funcCallB, Is.Not.Null);
        Assert.That(funcCallB.Name, Is.EqualTo("func_b"));
        Assert.That(funcCallB.Arguments, Has.Count.EqualTo(1));
        
        var funcCallC = funcCallA.Arguments[1] as FunctionCallNode;
        Assert.That(funcCallC, Is.Not.Null);
        Assert.That(funcCallC.Name, Is.EqualTo("func_c"));
        Assert.That(funcCallC.Arguments, Is.Empty);
    }
    
    [Test]
    public void VisitExpression_ComplexLogicalExpression_ParsesWithCorrectPrecedence() {
        var statement = GetFirstStatement("if (a > 5 && b < 10 || c == 0) {}");
        var ifNode = (IfNode)statement;
        var condition = ifNode.Condition as BinaryOpNode;
        
        // AST should be: ((a > 5 && b < 10) || c == 0) due to && having higher precedence than ||
        Assert.That(condition, Is.Not.Null);
        Assert.That(condition.Operator, Is.EqualTo(BinaryOperator.LogicalOr));
        
        // Left side of || should be the && expression
        var leftOfOr = condition.Left as BinaryOpNode;
        Assert.That(leftOfOr, Is.Not.Null);
        Assert.That(leftOfOr.Operator, Is.EqualTo(BinaryOperator.LogicalAnd));
        
        // Right side of || should be the == expression
        var rightOfOr = condition.Right as BinaryOpNode;
        Assert.That(rightOfOr, Is.Not.Null);
        Assert.That(rightOfOr.Operator, Is.EqualTo(BinaryOperator.Equal));
    }

    #endregion
}