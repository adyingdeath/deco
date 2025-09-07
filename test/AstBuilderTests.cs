using Antlr4.Runtime;
using Deco.Ast;
using NUnit.Framework;
using System.Linq;

namespace Deco.Tests.Ast;

[TestFixture]
public class AstBuilderTests {
    #region Helper Methods

    private AstNode BuildAst(string code) {
        var stream = CharStreams.fromString(code);
        var lexer = new DecoLexer(stream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new DecoParser(tokens);
        parser.BuildParseTree = true;
        var tree = parser.program();
        var astBuilder = new AstBuilder();
        return astBuilder.Visit(tree);
    }

    private T GetFirstStatement<T>(string code) where T : AstNode {
        var program = (ProgramNode)BuildAst(code);
        var function = program.Functions.FirstOrDefault();
        Assert.NotNull(function, "No function found in the program.");
        var statement = function.Body.Statements.FirstOrDefault();
        Assert.NotNull(statement, "No statement found in the function body.");
        Assert.IsInstanceOf<T>(statement, $"Statement is not of expected type {typeof(T).Name}");
        return (T)(object)statement!;
    }

    private T GetFirstExpression<T>(string expressionCode) where T : ExpressionNode {
        string code = $"void test() {{ {expressionCode}; }}";
        var statement = GetFirstStatement<ExpressionStatementNode>(code);
        Assert.IsInstanceOf<T>(statement.Expression, $"Expression is not of expected type {typeof(T).Name}");
        return (T)statement.Expression;
    }

    #endregion

    #region Program and Function Structure

    [Test]
    public void Build_EmptyProgram_ReturnsEmptyProgramNode() {
        var program = BuildAst("") as ProgramNode;
        Assert.NotNull(program);
        Assert.IsEmpty(program.Functions);
    }
    
    [Test]
    public void Build_ProgramWithMultipleFunctions_ParsesAllFunctions() {
        var code = @"
            void first() {}
            int second(int a) { return a; }
        ";
        var program = BuildAst(code) as ProgramNode;
        Assert.NotNull(program);
        Assert.AreEqual(2, program.Functions.Count);
        Assert.AreEqual("first", program.Functions[0].Name);
        Assert.AreEqual("second", program.Functions[1].Name);
    }

    [Test]
    public void Build_FunctionWithMixedModifiers_ParsesAllModifiersCorrectly() {
        var code = @"
            @tick
            @attribute(123, ""test"", myVar)
            @simple
            void myFunc() {}
        ";
        var program = BuildAst(code) as ProgramNode;
        var function = program.Functions[0];

        Assert.AreEqual(3, function.Modifiers.Count);
        
        Assert.AreEqual("tick", function.Modifiers[0].Name);
        Assert.IsEmpty(function.Modifiers[0].Parameters);

        Assert.AreEqual("attribute", function.Modifiers[1].Name);
        Assert.AreEqual(3, function.Modifiers[1].Parameters.Count);
        Assert.IsInstanceOf<LiteralNode>(function.Modifiers[1].Parameters[0]);
        Assert.IsInstanceOf<LiteralNode>(function.Modifiers[1].Parameters[1]);
        Assert.IsInstanceOf<IdentifierNode>(function.Modifiers[1].Parameters[2]);

        Assert.AreEqual("simple", function.Modifiers[2].Name);
        Assert.IsEmpty(function.Modifiers[2].Parameters);
    }
    
    #endregion

    #region Statements

    [Test]
    public void Build_EmptyBlock_CreatesBlockNodeWithNoStatements() {
        var code = "void test() { }";
        var program = BuildAst(code) as ProgramNode;
        var body = program.Functions[0].Body;
        Assert.NotNull(body);
        Assert.IsEmpty(body.Statements);
    }

    [Test]
    public void Build_StringWithEscapedQuotes_ParsesCorrectly() {
        var literal = GetFirstExpression<LiteralNode>("\"hello \\\"world\\\"!\"");
        Assert.AreEqual(LiteralType.String, literal.Type);
        Assert.AreEqual("hello \\\"world\\\"!", literal.Value);
    }

    [Test]
    public void Build_CommandWithSpecialCharacters_ParsesCorrectly() {
        var command = GetFirstStatement<CommandNode>("void t() { @`say JSON: {\"text\":\"hello\"}`; }");
        Assert.AreEqual("@`say JSON: {\"text\":\"hello\"}`", command.Command);
    }

    [Test]
    public void Build_IfElseIfElseChain_CreatesCorrectlyNestedStructure() {
        var code = @"
            void test() {
                if (c1) {
                } else if (c2) {
                } else if (c3) {
                } else {
                }
            }
        ";
        var rootIf = GetFirstStatement<IfNode>(code);
        
        // Level 1: if (c1)
        Assert.AreEqual("c1", ((IdentifierNode)rootIf.Condition).Name);
        Assert.NotNull(rootIf.ElseBlock);
        
        // Level 2: else if (c2)
        var elseIf1 = rootIf.ElseBlock.Statements[0] as IfNode;
        Assert.NotNull(elseIf1);
        Assert.AreEqual("c2", ((IdentifierNode)elseIf1.Condition).Name);
        Assert.NotNull(elseIf1.ElseBlock);

        // Level 3: else if (c3)
        var elseIf2 = elseIf1.ElseBlock.Statements[0] as IfNode;
        Assert.NotNull(elseIf2);
        Assert.AreEqual("c3", ((IdentifierNode)elseIf2.Condition).Name);
        Assert.NotNull(elseIf2.ElseBlock);

        // Level 4: final else
        Assert.IsEmpty(elseIf2.ElseBlock.Statements);
    }

    #endregion

    #region Expressions and Operators

    [Test]
    public void Build_ParenthesizedExpression_OverridesDefaultPrecedence() {
        // (a + b) * c
        var expr = GetFirstExpression<BinaryOpNode>("(a + b) * c");

        Assert.AreEqual(BinaryOperator.Multiply, expr.Operator);
        Assert.IsInstanceOf<IdentifierNode>(expr.Right);
        Assert.AreEqual("c", ((IdentifierNode)expr.Right).Name);

        var left = expr.Left as BinaryOpNode;
        Assert.NotNull(left);
        Assert.AreEqual(BinaryOperator.Add, left.Operator);
        Assert.AreEqual("a", ((IdentifierNode)left.Left).Name);
        Assert.AreEqual("b", ((IdentifierNode)left.Right).Name);
    }

    [Test]
    public void Build_AllLogicalOperators_RespectsPrecedenceAndAssociativity() {
        // a || b && c || d  ->  (a || (b && c)) || d
        var expr = GetFirstExpression<BinaryOpNode>("a || b && c || d");
        
        Assert.AreEqual(BinaryOperator.LogicalOr, expr.Operator); // The final OR
        Assert.AreEqual("d", ((IdentifierNode)expr.Right).Name);

        var leftOr = expr.Left as BinaryOpNode; // (a || (b && c))
        Assert.NotNull(leftOr);
        Assert.AreEqual(BinaryOperator.LogicalOr, leftOr.Operator);
        Assert.AreEqual("a", ((IdentifierNode)leftOr.Left).Name);

        var rightOr = leftOr.Right as BinaryOpNode; // (b && c)
        Assert.NotNull(rightOr);
        Assert.AreEqual(BinaryOperator.LogicalAnd, rightOr.Operator);
        Assert.AreEqual("b", ((IdentifierNode)rightOr.Left).Name);
        Assert.AreEqual("c", ((IdentifierNode)rightOr.Right).Name);
    }

    [Test]
    public void Build_AllComparisonOperators_CreatesCorrectNodes() {
        var ops = new[] { "==", "!=", ">", "<", ">=", "<=" };
        var expected = new[] {
            BinaryOperator.Equal, BinaryOperator.NotEqual,
            BinaryOperator.GreaterThan, BinaryOperator.LessThan,
            BinaryOperator.GreaterThanOrEqual, BinaryOperator.LessThanOrEqual
        };

        for (int i = 0; i < ops.Length; i++) {
            var expr = GetFirstExpression<BinaryOpNode>($"a {ops[i]} b");
            Assert.AreEqual(expected[i], expr.Operator, $"Failed for operator {ops[i]}");
        }
    }

    [Test]
    public void Build_ChainedUnaryOperators_NestsCorrectly() {
        // !-!x -> !(-(!x))
        var expr = GetFirstExpression<UnaryOpNode>("!-!x");
        Assert.AreEqual(UnaryOperator.LogicalNot, expr.Operator);
        
        var inner1 = expr.Operand as UnaryOpNode;
        Assert.NotNull(inner1);
        Assert.AreEqual(UnaryOperator.Negate, inner1.Operator);

        var inner2 = inner1.Operand as UnaryOpNode;
        Assert.NotNull(inner2);
        Assert.AreEqual(UnaryOperator.LogicalNot, inner2.Operator);

        var operand = inner2.Operand as IdentifierNode;
        Assert.NotNull(operand);
        Assert.AreEqual("x", operand.Name);
    }

    [Test]
    public void Build_NestedFunctionCalls_CreatesCorrectTree() {
        // f(g(1), h(a, "s"))
        var call = GetFirstExpression<FunctionCallNode>("f(g(1), h(a, \"s\"))");
        
        Assert.AreEqual("f", call.Name);
        Assert.AreEqual(2, call.Arguments.Count);
        
        var arg1 = call.Arguments[0] as FunctionCallNode;
        Assert.NotNull(arg1);
        Assert.AreEqual("g", arg1.Name);
        Assert.AreEqual(1, arg1.Arguments.Count);
        var g_param = arg1.Arguments[0] as LiteralNode;
        Assert.AreEqual("1", g_param.Value);
        
        var arg2 = call.Arguments[1] as FunctionCallNode;
        Assert.NotNull(arg2);
        Assert.AreEqual("h", arg2.Name);
        Assert.AreEqual(2, arg2.Arguments.Count);
        Assert.IsInstanceOf<IdentifierNode>(arg2.Arguments[0]);
        var h_param2 = arg2.Arguments[1] as LiteralNode;
        Assert.AreEqual("s", h_param2.Value);
    }
    
    #endregion
    
    #region Complex and Nested Structures

    [Test]
    public void Build_WhileLoopContainingIfElse_ParsesCorrectly() {
        var code = @"
            void test() {
                while (running) {
                    if (check()) {
                        a = 1;
                    } else {
                        b = 2;
                    }
                }
            }
        ";
        var whileNode = GetFirstStatement<WhileNode>(code);
        Assert.AreEqual("running", ((IdentifierNode)whileNode.Condition).Name);
        Assert.AreEqual(1, whileNode.Body.Statements.Count);
        
        var ifNode = whileNode.Body.Statements[0] as IfNode;
        Assert.NotNull(ifNode);
        Assert.AreEqual("check", ((FunctionCallNode)ifNode.Condition).Name);
        
        Assert.AreEqual(1, ifNode.ThenBlock.Statements.Count);
        Assert.IsInstanceOf<AssignmentNode>(ifNode.ThenBlock.Statements[0]);
        
        Assert.NotNull(ifNode.ElseBlock);
        Assert.AreEqual(1, ifNode.ElseBlock.Statements.Count);
        Assert.IsInstanceOf<AssignmentNode>(ifNode.ElseBlock.Statements[0]);
    }
    
    [Test]
    public void Build_IfStatementContainingWhileLoop_ParsesCorrectly() {
        var code = @"
            void test() {
                if (shouldLoop) {
                    while (i > 0) {
                        i = i - 1;
                    }
                }
            }
        ";
        var ifNode = GetFirstStatement<IfNode>(code);
        Assert.AreEqual("shouldLoop", ((IdentifierNode)ifNode.Condition).Name);
        Assert.AreEqual(1, ifNode.ThenBlock.Statements.Count);
        
        var whileNode = ifNode.ThenBlock.Statements[0] as WhileNode;
        Assert.NotNull(whileNode);
        Assert.IsInstanceOf<BinaryOpNode>(whileNode.Condition);
        Assert.AreEqual(1, whileNode.Body.Statements.Count);
    }

    [Test]
    public void Build_GiantComplexExpression_ParsesCorrectlyWithoutCrashing() {
        var code = "x = -f(a, b*c) + (d >= 5 && !g(h() || k)) / 2;";
        var assignment = GetFirstStatement<AssignmentNode>($"void t() {{ {code} }}");
        
        // We mainly want to ensure this parses without error.
        // A deep check proves the structure.
        Assert.AreEqual("x", assignment.Variable);
        var expr = assignment.Expression as BinaryOpNode;
        
        // Top level is addition
        Assert.NotNull(expr);
        Assert.AreEqual(BinaryOperator.Add, expr.Operator);

        // Left side is unary negate
        var left = expr.Left as UnaryOpNode;
        Assert.NotNull(left);
        Assert.AreEqual(UnaryOperator.Negate, left.Operator);
        Assert.IsInstanceOf<FunctionCallNode>(left.Operand);

        // Right side is division
        var right = expr.Right as BinaryOpNode;
        Assert.NotNull(right);
        Assert.AreEqual(BinaryOperator.Divide, right.Operator);
        Assert.IsInstanceOf<LiteralNode>(right.Right);
        
        // The left side of the division is a complex boolean expression
        var boolExpr = right.Left as BinaryOpNode;
        Assert.NotNull(boolExpr);
        Assert.AreEqual(BinaryOperator.LogicalAnd, boolExpr.Operator);
    }
    
    [Test]
    public void Build_CodeWithExcessiveWhitespaceAndNewlines_ParsesCorrectly() {
        var code = @"
            int 
            messyFunc
            ( 
                int arg1, 
                
                string arg2
            ) 
            {

                if      (arg1 > 10) 
                
                {
                    return 
                        (arg1   * 2)
                        
                        ;
                }


            }
        ";
        
        var program = BuildAst(code) as ProgramNode;
        Assert.NotNull(program);
        Assert.AreEqual(1, program.Functions.Count);
        var function = program.Functions[0];
        Assert.AreEqual("messyFunc", function.Name);
        Assert.AreEqual(2, function.Arguments.Count);
        Assert.AreEqual(1, function.Body.Statements.Count);
        Assert.IsInstanceOf<IfNode>(function.Body.Statements[0]);
    }

    #endregion
}