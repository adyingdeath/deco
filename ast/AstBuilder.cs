using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Deco.Ast;

public class AstBuilder : DecoBaseVisitor<AstNode> {
    public override AstNode VisitProgram(DecoParser.ProgramContext context) {
        var functions = new List<FunctionNode>();

        foreach (var functionContext in context.function()) {
            var function = VisitFunction(functionContext) as FunctionNode;
            if (function != null) {
                functions.Add(function);
            }
        }

        return new ProgramNode(functions, context.Start.Line, context.Start.Column);
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

        return new ModifierNode(context.name.Text, parameters, context.Start.Line, context.Start.Column);
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

        return new FunctionNode(
            modifiers,
            context.type.Text,
            context.name.Text,
            arguments,
            body,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitArgument(DecoParser.ArgumentContext context) {
        return new ArgumentNode(
            context.type.Text,
            context.name.Text,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitStatement(DecoParser.StatementContext context) {
        // Statement can be:
        // 1. COMMAND ';'
        // 2. expression ';'
        // 3. variableDefinition ';'
        // 4. assignment ';'
        // 5. return_statement
        // 6. if_statement
        // 7. while_statement

        if (context.COMMAND() != null) {
            // COMMAND statement
            return new CommandNode(
                context.COMMAND().GetText(),
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.expression() != null) {
            // Expression statement
            var expression = Visit(context.expression()) as ExpressionNode;
            if (expression != null) {
                var expressionStmt = new ExpressionStatementNode(
                    expression,
                    context.Start.Line,
                    context.Start.Column
                );
                return expressionStmt;
            }
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
            elseBlock = new BlockNode([ifElseStatement]);
        } else if (context.block().Length > 1) {
            elseBlock = Visit(context.block(1)) as BlockNode;
        }

        return new IfNode(condition, thenBlock, elseBlock, context.Start.Line, context.Start.Column);
    }

    public override AstNode VisitWhile_statement(DecoParser.While_statementContext context) {
        var condition = Visit(context.expression()) as ExpressionNode;
        if (condition == null) {
            throw new InvalidOperationException("While statement must have a condition");
        }

        var body = Visit(context.block()) as BlockNode;
        if (body == null) {
            throw new InvalidOperationException("While statement must have a body");
        }

        return new WhileNode(condition, body, context.Start.Line, context.Start.Column);
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
        var expressions = new List<ExpressionNode>();

        foreach (var andExpr in context.and_expr()) {
            var expr = Visit(andExpr) as ExpressionNode;
            if (expr != null) {
                expressions.Add(expr);
            }
        }

        // If there's only one expression, return it directly
        if (expressions.Count == 1) {
            return expressions[0];
        }

        // Build chained binary operations
        var result = expressions[0];
        for (int i = 1; i < expressions.Count; i++) {
            result = new BinaryOpNode(
                result,
                BinaryOperator.LogicalOr,
                expressions[i],
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitAnd_expr(DecoParser.And_exprContext context) {
        var expressions = new List<ExpressionNode>();

        foreach (var eqExpr in context.eq_expr()) {
            var expr = Visit(eqExpr) as ExpressionNode;
            if (expr != null) {
                expressions.Add(expr);
            }
        }

        // If there's only one expression, return it directly
        if (expressions.Count == 1) {
            return expressions[0];
        }

        // Build chained binary operations
        var result = expressions[0];
        for (int i = 1; i < expressions.Count; i++) {
            result = new BinaryOpNode(
                result,
                BinaryOperator.LogicalAnd,
                expressions[i],
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitEq_expr(DecoParser.Eq_exprContext context) {
        var expressions = new List<ExpressionNode>();
        var operators = new List<BinaryOperator>();

        if (Visit(context.rel_expr(0)) is not ExpressionNode expr0) {
            throw new InvalidOperationException("Eq expression must have a valid expression");
        }
        expressions.Add(expr0);

        for (int i = 0; i < context.rel_expr().Length - 1; i++) {
            // Get the operator token at position i*2 + 1 (between expressions)
            var opToken = (TerminalNodeImpl)context.GetChild(i * 2 + 1);
            var op = opToken.Symbol.Text switch {
                "==" => BinaryOperator.Equal,
                "!=" => BinaryOperator.NotEqual,
                _ => throw new InvalidOperationException($"Unknown equality operator: {opToken.Symbol.Text}")
            };
            operators.Add(op);

            if (Visit(context.rel_expr(i + 1)) is ExpressionNode expr) {
                expressions.Add(expr);
            } else {
                throw new InvalidOperationException("Eq expression must have a valid expression");
            }
        }

        // If there's only one expression, return it directly
        if (expressions.Count == 1) {
            return expressions[0];
        }

        // Build binary operations
        var result = expressions[0];
        for (int i = 0; i < operators.Count; i++) {
            result = new BinaryOpNode(
                result,
                operators[i],
                expressions[i + 1],
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitVariableDefinition(DecoParser.VariableDefinitionContext context) {
        return new VariableDefinitionNode(
            context.type.Text,
            context.name.Text,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitAssignment(DecoParser.AssignmentContext context) {
        var expression =
            Visit(context.expression()) as ExpressionNode
            ?? throw new InvalidOperationException("Assignment must have a valid expression");

        return new AssignmentNode(
            context.IDENTIFIER().GetText(),
            expression,
            context.Start.Line,
            context.Start.Column
        );
    }

    public override AstNode VisitFunctionCall(DecoParser.FunctionCallContext context) {
        var arguments = new List<ExpressionNode>();

        if (context.expression() != null) {
            foreach (var expr in context.expression()) {
                if (Visit(expr) is ExpressionNode expression) {
                    arguments.Add(expression);
                }
            }
        }

        return new FunctionCallNode(
            context.name.Text,
            arguments,
            context.Start.Line,
            context.Start.Column
        );
    }

    // Expression visiting - need to extend with all expression types
    public override AstNode VisitRel_expr(DecoParser.Rel_exprContext context) {
        var expressions = new List<ExpressionNode>();
        var operators = new List<BinaryOperator>();

        if (Visit(context.add_expr(0)) is not ExpressionNode expr0) {
            throw new InvalidOperationException("Rel expression must have a valid expression");
        }
        expressions.Add(expr0);

        for (int i = 0; i < context.add_expr().Length - 1; i++) {
            // Get the operator token at position i*2 + 1 (between expressions)
            var opNode = (TerminalNodeImpl)context.GetChild(i * 2 + 1);
            BinaryOperator op = opNode.Symbol.Text switch {
                ">=" => BinaryOperator.GreaterThanOrEqual,
                "<=" => BinaryOperator.LessThanOrEqual,
                ">" => BinaryOperator.GreaterThan,
                "<" => BinaryOperator.LessThan,
                _ => throw new InvalidOperationException($"Unknown relational operator: {opNode.Symbol.Text}")
            };
            operators.Add(op);

            if (Visit(context.add_expr(i + 1)) is ExpressionNode expr) {
                expressions.Add(expr);
            } else {
                throw new InvalidOperationException("Rel expression must have a valid expression");
            }
        }

        // If there's only one expression, return it directly
        if (expressions.Count == 1) {
            return expressions[0];
        }

        // Build binary operations from left to right
        var result = expressions[0];
        for (int i = 0; i < operators.Count; i++) {
            result = new BinaryOpNode(
                result,
                operators[i],
                expressions[i + 1],
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitAdd_expr(DecoParser.Add_exprContext context) {
        var expressions = new List<ExpressionNode>();
        var operators = new List<BinaryOperator>();

        if (Visit(context.mul_expr(0)) is not ExpressionNode expr0) {
            throw new InvalidOperationException("Add expression must have a valid expression");
        }
        expressions.Add(expr0);

        for (int i = 0; i < context.mul_expr().Length - 1; i++) {
            // Get the operator token at position i*2 + 1 (between expressions)
            var opNode = (TerminalNodeImpl)context.GetChild(i * 2 + 1);
            BinaryOperator op = opNode.Symbol.Text switch {
                "+" => BinaryOperator.Add,
                "-" => BinaryOperator.Subtract,
                _ => throw new InvalidOperationException($"Unknown additive operator: {opNode.Symbol.Text}")
            };
            operators.Add(op);

            if (Visit(context.mul_expr(i + 1)) is ExpressionNode expr) {
                expressions.Add(expr);
            } else {
                throw new InvalidOperationException("Add expression must have a valid expression");
            }
        }

        // If there's only one expression, return it directly
        if (expressions.Count == 1) {
            return expressions[0];
        }

        // Build binary operations from left to right
        var result = expressions[0];
        for (int i = 0; i < operators.Count; i++) {
            result = new BinaryOpNode(
                result,
                operators[i],
                expressions[i + 1],
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitMul_expr(DecoParser.Mul_exprContext context) {
        var expressions = new List<ExpressionNode>();
        var operators = new List<BinaryOperator>();

        if (Visit(context.unary_expr(0)) is not ExpressionNode expr0) {
            throw new InvalidOperationException("Mul expression must have a valid expression");
        }
        expressions.Add(expr0);

        for (int i = 0; i < context.unary_expr().Length - 1; i++) {
            // Get the operator token at position i*2 + 1 (between expressions)
            var opNode = (TerminalNodeImpl)context.GetChild(i * 2 + 1);
            BinaryOperator op = opNode.Symbol.Text switch {
                "*" => BinaryOperator.Multiply,
                "/" => BinaryOperator.Divide,
                _ => throw new InvalidOperationException($"Unknown multiplicative operator: {opNode.Symbol.Text}")
            };
            operators.Add(op);

            if (Visit(context.unary_expr(i + 1)) is ExpressionNode expr) {
                expressions.Add(expr);
            } else {
                throw new InvalidOperationException("Mul expression must have a valid expression");
            }
        }

        // If there's only one expression, return it directly
        if (expressions.Count == 1) {
            return expressions[0];
        }

        // Build binary operations from left to right
        var result = expressions[0];
        for (int i = 0; i < operators.Count; i++) {
            result = new BinaryOpNode(
                result,
                operators[i],
                expressions[i + 1],
                context.Start.Line,
                context.Start.Column
            );
        }

        return result;
    }

    public override AstNode VisitUnary_expr(DecoParser.Unary_exprContext context) {
        if (context.GetChild(0) is TerminalNodeImpl opNode && (opNode.Symbol.Text == "!" || opNode.Symbol.Text == "-")) {
            var op = opNode.Symbol.Text == "!" ? UnaryOperator.LogicalNot : UnaryOperator.Negate;
            var expr = Visit(context.unary_expr()) as ExpressionNode;
            if (expr == null) {
                throw new InvalidOperationException("Unary expression must have a valid operand");
            }
            return new UnaryOpNode(op, expr, context.Start.Line, context.Start.Column);
        } else {
            return Visit(context.primary());
        }
    }

    public override AstNode VisitPrimary(DecoParser.PrimaryContext context) {
        if (context.NUMBER() != null) {
            return new LiteralNode(
                LiteralType.Number,
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
                LiteralType.String,
                text,
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.TRUE() != null) {
            return new LiteralNode(
                LiteralType.Boolean,
                "true",
                context.Start.Line,
                context.Start.Column
            );
        }

        if (context.FALSE() != null) {
            return new LiteralNode(
                LiteralType.Boolean,
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
