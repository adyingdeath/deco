using Antlr4.Runtime.Tree;
using Deco.Types;

namespace Deco.Compiler.Ast;

public class AstBuilder : DecoBaseVisitor<AstNode> {
    public override AstNode VisitProgram(DecoParser.ProgramContext context) {
        var varDefs = new List<VariableDefinitionNode>();
        var functions = new List<FunctionNode>();

        foreach (var varDefContext in context.variableDefinition()) {
            if (VisitVariableDefinition(varDefContext) is VariableDefinitionNode varDef) {
                varDefs.Add(varDef);
            }
        }

        foreach (var functionContext in context.function()) {
            if (VisitFunction(functionContext) is FunctionNode function) {
                functions.Add(function);
            }
        }

        return new ProgramNode(varDefs, functions, context.Start.Line, context.Start.Column);
    }

    public override AstNode VisitModifier(DecoParser.ModifierContext context) {
        var parameters = new List<ExpressionNode>();

        if (context.expression() != null) {
            foreach (var expr in context.expression()) {
                var expression = Visit(expr) as ExpressionNode;
                if (expression != null) {
                    parameters.Add(expression);
                }
            }
        }

        var name = new IdentifierNode(
            context.name.Text,
            context.name.Line,
            context.name.Column
        );

        return new ModifierNode(name, parameters, context.Start.Line, context.Start.Column);
    }

    public override AstNode VisitFunction(DecoParser.FunctionContext context) {
        var modifiers = new List<ModifierNode>();
        var arguments = new List<ArgumentNode>();

        // Visit modifiers
        foreach (var modifierContext in context.modifier()) {
            var modifier = VisitModifier(modifierContext) as ModifierNode;
            if (modifier != null) {
                modifiers.Add(modifier);
            }
        }

        // Visit arguments if present
        if (context.arguments() != null) {
            foreach (var arg in context.arguments().argument()) {
                arguments.Add(VisitArgument(arg) as ArgumentNode ?? throw new InvalidOperationException("Invalid argument"));
            }
        }

        // Visit block
        var body = VisitBlock(context.block()) as BlockNode;
        if (body == null) {
            throw new InvalidOperationException("Function must have a body");
        }

        var name = new IdentifierNode(
            context.name.Text,
            context.name.Line,
            context.name.Column
        );

        return new FunctionNode(
            modifiers,
            new UnresolvedType(context.type.Text),
            name,
            arguments,
            body,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitArgument(DecoParser.ArgumentContext context) {
        var name = new IdentifierNode(
            context.name.Text,
            context.name.Line,
            context.name.Column
        );
        return new ArgumentNode(
            context.type.Text,
            name,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitStatement(DecoParser.StatementContext context) {
        /* 
        Statement can be:
        1. COMMAND ';'
        2. expression ';'
        3. variableDefinition ';'
        4. assignment ';'
        5. return_statement
        6. if_statement
        7. while_statement
        8. for_statement
        */

        if (context.COMMAND() != null) {
            // COMMAND statement
            return new CommandNode(
                context.COMMAND().GetText()[2..^1],
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.expression() != null) {
            // Expression statement
            var expression =
                Visit(context.expression()) as ExpressionNode
                ?? throw new InvalidOperationException("Expression statement must contain a valid expression.");

            return new ExpressionStatementNode(
                expression,
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.variableDefinition() != null) {
            return VisitVariableDefinition(context.variableDefinition());
        }

        if (context.assignment() != null) {
            return VisitAssignment(context.assignment());
        }

        if (context.return_statement() != null) {
            return VisitReturn_statement(context.return_statement());
        }

        if (context.if_statement() != null) {
            return VisitIf_statement(context.if_statement());
        }

        if (context.while_statement() != null) {
            return VisitWhile_statement(context.while_statement());
        }

        if (context.for_statement() != null) {
            return VisitFor_statement(context.for_statement());
        }

        throw new InvalidOperationException("Unknown statement type");
    }

    public override AstNode VisitReturn_statement(DecoParser.Return_statementContext context) {
        ExpressionNode? expression = null;
        if (context.expression() != null) {
            expression = Visit(context.expression()) as ExpressionNode;
        }

        return new ReturnNode(expression, context.Start.Line, context.Start.Column);
    }

    public override AstNode VisitIf_statement(DecoParser.If_statementContext context) {
        if (Visit(context.expression()) is not ExpressionNode condition) {
            throw new InvalidOperationException("If statement must have a condition");
        }

        var thenBlock =
            Visit(context.block(0)) as BlockNode
            ?? throw new InvalidOperationException("If statement must have a then block");

        BlockNode? elseBlock = null;
        if (context.if_statement() != null) {
            // Create a block node and set the if else statement to be the only statement in the block node
            var ifElseStatement =
                VisitIf_statement(context.if_statement()) as StatementNode
                ?? throw new InvalidOperationException("Else if statement is invalid");
            elseBlock = new BlockNode(
                [ifElseStatement],
                context.if_statement().Start.Line,
                context.if_statement().Start.Column
            );
        } else if (context.block().Length > 1) {
            elseBlock = Visit(context.block(1)) as BlockNode;
        }

        return new IfNode(condition, thenBlock, elseBlock, context.Start.Line, context.Start.Column);
    }

    public override AstNode VisitWhile_statement(DecoParser.While_statementContext context) {
        var condition =
            Visit(context.expression()) as ExpressionNode
            ?? throw new InvalidOperationException("While statement must have a condition");
        var body =
            Visit(context.block()) as BlockNode
            ?? throw new InvalidOperationException("While statement must have a body");
        return new WhileNode(condition, body, context.Start.Line, context.Start.Column);
    }

    public override AstNode VisitFor_statement(DecoParser.For_statementContext context) {
        var condition =
            Visit(context.cond) as ExpressionNode
            ?? throw new InvalidOperationException("For statement must have a condition");
        StatementNode? init = null;
        if (context.init != null) {
            if (context.init.expression() != null) {
                var expr = (ExpressionNode)Visit(context.init.expression());
                init = new ExpressionStatementNode(expr, context.Start.Line, context.Start.Column);
            } else if (context.init.variableDefinition() != null) {
                init = (VariableDefinitionNode)Visit(context.init.variableDefinition());
            } else if (context.init.assignment() != null) {
                init = (AssignmentNode)Visit(context.init.assignment());
            }
        }
        StatementNode? iter = null;
        if (context.iter != null) {
            if (context.iter.expression() != null) {
                var expr = (ExpressionNode)Visit(context.iter.expression());
                iter = new ExpressionStatementNode(expr, context.Start.Line, context.Start.Column);
            } else if (context.iter.variableDefinition() != null) {
                iter = (VariableDefinitionNode)Visit(context.iter.variableDefinition());
            } else if (context.iter.assignment() != null) {
                iter = (AssignmentNode)Visit(context.iter.assignment());
            }
        }
        var body =
            Visit(context.block()) as BlockNode
            ?? throw new InvalidOperationException("For statement must have a body");
        return new ForNode(init, condition, iter, body, context.Start.Line, context.Start.Column);
    }

    public override AstNode VisitBlock(DecoParser.BlockContext context) {
        var statements = new List<StatementNode>();

        foreach (var statementContext in context.statement()) {
            var statement = Visit(statementContext) as StatementNode;
            if (statement != null) {
                statements.Add(statement);
            }
        }

        return new BlockNode(statements, context.Start.Line, context.Start.Column);
    }

    public override AstNode VisitExpression(DecoParser.ExpressionContext context) {
        return VisitOr_expr(context.or_expr());
    }

    public override AstNode VisitOr_expr(DecoParser.Or_exprContext context) {
        // Start with the leftmost expression
        var result =
            Visit(context.and_expr(0)) as ExpressionNode
            ?? throw new InvalidOperationException("Expression cannot be null");

        // Iteratively build the left-associative binary operation tree
        for (int i = 1; i < context.and_expr().Length; i++) {
            var right =
                Visit(context.and_expr(i)) as ExpressionNode
                ?? throw new InvalidOperationException("Expression cannot be null");

            result = new BinaryOpNode(
                result,
                BinaryOperator.LogicalOr,
                right,
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitAnd_expr(DecoParser.And_exprContext context) {
        // Start with the leftmost expression
        var result =
            Visit(context.eq_expr(0)) as ExpressionNode
            ?? throw new InvalidOperationException("Expression cannot be null");

        // Iteratively build the left-associative binary operation tree
        for (int i = 1; i < context.eq_expr().Length; i++) {
            var right =
                Visit(context.eq_expr(i)) as ExpressionNode
                ?? throw new InvalidOperationException("Expression cannot be null");

            result = new BinaryOpNode(
                result,
                BinaryOperator.LogicalAnd,
                right,
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitEq_expr(DecoParser.Eq_exprContext context) {
        // Start with the leftmost expression
        var result =
            Visit(context.rel_expr(0)) as ExpressionNode
            ?? throw new InvalidOperationException("Expression cannot be null");

        // Iteratively build the left-associative binary operation tree
        for (int i = 1; i < context.rel_expr().Length; i++) {
            // Get the operator
            var opToken = (TerminalNodeImpl)context.GetChild(i * 2 - 1);
            var op = opToken.Symbol.Text switch {
                "==" => BinaryOperator.Equal,
                "!=" => BinaryOperator.NotEqual,
                _ => throw new InvalidOperationException($"Unknown equality operator: {opToken.Symbol.Text}")
            };

            var right =
                Visit(context.rel_expr(i)) as ExpressionNode
                ?? throw new InvalidOperationException("Expression cannot be null");

            result = new BinaryOpNode(
                result,
                op,
                right,
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitRel_expr(DecoParser.Rel_exprContext context) {
        // Start with the leftmost expression
        var result =
            Visit(context.add_expr(0)) as ExpressionNode
            ?? throw new InvalidOperationException("Expression cannot be null");

        // Iteratively build the left-associative binary operation tree
        for (int i = 1; i < context.add_expr().Length; i++) {
            // Get the operator
            var opNode = (TerminalNodeImpl)context.GetChild(i * 2 - 1);
            BinaryOperator op = opNode.Symbol.Text switch {
                ">=" => BinaryOperator.GreaterThanOrEqual,
                "<=" => BinaryOperator.LessThanOrEqual,
                ">" => BinaryOperator.GreaterThan,
                "<" => BinaryOperator.LessThan,
                _ => throw new InvalidOperationException($"Unknown relational operator: {opNode.Symbol.Text}")
            };

            var right =
                Visit(context.add_expr(i)) as ExpressionNode
                ?? throw new InvalidOperationException("Expression cannot be null");

            result = new BinaryOpNode(
                result,
                op,
                right,
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitAdd_expr(DecoParser.Add_exprContext context) {
        // Start with the leftmost expression
        var result =
            Visit(context.mul_expr(0)) as ExpressionNode
            ?? throw new InvalidOperationException("Expression cannot be null");

        // Iteratively build the left-associative binary operation tree
        for (int i = 1; i < context.mul_expr().Length; i++) {
            // Get the operator
            var opNode = (TerminalNodeImpl)context.GetChild(i * 2 - 1);
            BinaryOperator op = opNode.Symbol.Text switch {
                "+" => BinaryOperator.Add,
                "-" => BinaryOperator.Subtract,
                _ => throw new InvalidOperationException($"Unknown additive operator: {opNode.Symbol.Text}")
            };

            var right =
                Visit(context.mul_expr(i)) as ExpressionNode
                ?? throw new InvalidOperationException("Expression cannot be null");

            result = new BinaryOpNode(
                result,
                op,
                right,
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitMul_expr(DecoParser.Mul_exprContext context) {
        // Start with the leftmost expression
        var result =
            Visit(context.unary_expr(0)) as ExpressionNode
            ?? throw new InvalidOperationException("Expression cannot be null");

        // Iteratively build the left-associative binary operation tree
        for (int i = 1; i < context.unary_expr().Length; i++) {
            // Get the operator between the (i-1)th and ith expression
            var opNode = (TerminalNodeImpl)context.GetChild(i * 2 - 1);
            BinaryOperator op = opNode.Symbol.Text switch {
                "*" => BinaryOperator.Multiply,
                "/" => BinaryOperator.Divide,
                _ => throw new InvalidOperationException($"Unknown multiplicative operator: {opNode.Symbol.Text}")
            };

            var right =
                Visit(context.unary_expr(i)) as ExpressionNode
                ?? throw new InvalidOperationException("Expression cannot be null");

            result = new BinaryOpNode(
                result,
                op,
                right,
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitUnary_expr(DecoParser.Unary_exprContext context) {
        if (context.primary() != null) {
            return Visit(context.primary());
        } else {
            var op = context.op.Text switch {
                "!" => UnaryOperator.LogicalNot,
                "-" => UnaryOperator.Negate,
                _ => throw new InvalidOperationException($"Unknown unary operator: {context.op.Text}")
            };
            var expr =
                Visit(context.unary_expr()) as ExpressionNode
                ?? throw new InvalidOperationException("Unary expression must have a valid operand");
            return new UnaryOpNode(op, expr, context.Start.Line, context.Start.Column);
        }
    }

    public override AstNode VisitVariableDefinition(DecoParser.VariableDefinitionContext context) {
        ExpressionNode? initExpr = null;
        if (context.expression() != null) {
            initExpr = (ExpressionNode)Visit(context.expression());
        }

        var name = new IdentifierNode(
            context.name.Text,
            context.name.Line,
            context.name.Column
        );

        // Create VariableDefinitionNode without type - type info is stored in symbol table
        return new VariableDefinitionNode(
            name,
            initExpr,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitAssignment(DecoParser.AssignmentContext context) {
        var variable = new IdentifierNode(
            context.IDENTIFIER().GetText(),
            context.IDENTIFIER().Symbol.Line,
            context.IDENTIFIER().Symbol.Column
        );
        var expression =
            Visit(context.expression()) as ExpressionNode
            ?? throw new InvalidOperationException("Assignment must have a valid expression");

        return new AssignmentNode(
            variable,
            expression,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitFunctionCall(DecoParser.FunctionCallContext context) {
        var arguments = new List<ExpressionNode>();

        if (context.expression() != null) {
            foreach (var expr in context.expression()) {
                ExpressionNode expression =
                    (ExpressionNode)Visit(expr)
                    ?? throw new InvalidOperationException("Invalid expression");
                arguments.Add(expression);
            }
        }

        var function = new IdentifierNode(
            context.name.Text,
            context.name.Line,
            context.name.Column
        );

        return new FunctionCallNode(
            function,
            arguments,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitPrimary(DecoParser.PrimaryContext context) {
        if (context.NUMBER() != null) {
            return new LiteralNode(
                TypeUtils.IntType,
                context.NUMBER().GetText(),
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.STRING() != null) {
            var text = context.STRING().GetText();
            // Remove quotes
            text = text[1..^1];
            return new LiteralNode(
                TypeUtils.StringType,
                text,
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.TRUE() != null) {
            return new LiteralNode(
                TypeUtils.BoolType,
                "true",
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.FALSE() != null) {
            return new LiteralNode(
                TypeUtils.BoolType,
                "false",
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.IDENTIFIER() != null) {
            return new IdentifierNode(
                context.IDENTIFIER().GetText(),
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.functionCall() != null) {
            return VisitFunctionCall(context.functionCall());
        }

        if (context.expression() != null) {
            return Visit(context.expression());
        }

        throw new InvalidOperationException("Unknown primary expression type");
    }
}
