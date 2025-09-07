namespace Deco.Ast;

public class BaseAstVisitor<T> : IAstVisitor<T> {
    public virtual T VisitProgram(ProgramNode node) {
        foreach (var function in node.Functions) {
            Visit(function);
        }
        return default!;
    }

    public virtual T VisitFunction(FunctionNode node) {
        foreach (var modifier in node.Modifiers) {
            Visit(modifier);
        }

        foreach (var arg in node.Arguments) {
            Visit(arg);
        }

        return Visit(node.Body);
    }

    public virtual T VisitModifier(ModifierNode node) {
        foreach (var param in node.Parameters) {
            Visit(param);
        }
        return default!;
    }

    public virtual T VisitArgument(ArgumentNode node) {
        return default!;
    }

    public virtual T VisitExpressionStatement(ExpressionStatementNode node) {
        return default!;
    }

    public virtual T VisitCommand(CommandNode node) {
        return default!;
    }

    public virtual T VisitVariableDefinition(VariableDefinitionNode node) {
        return default!;
    }

    public virtual T VisitAssignment(AssignmentNode node) {
        return Visit(node.Expression);
    }

    public virtual T VisitReturn(ReturnNode node) {
        if (node.Expression != null) {
            return Visit(node.Expression);
        }
        return default!;
    }

    public virtual T VisitIf(IfNode node) {
        Visit(node.Condition);
        Visit(node.ThenBlock);
        if (node.ElseBlock != null) {
            Visit(node.ElseBlock);
        }
        return default!;
    }

    public virtual T VisitWhile(WhileNode node) {
        Visit(node.Condition);
        return Visit(node.Body);
    }

    public virtual T VisitBlock(BlockNode node) {
        foreach (var statement in node.Statements) {
            Visit(statement);
        }
        return default!;
    }

    public virtual T VisitBinaryOp(BinaryOpNode node) {
        Visit(node.Left);
        return Visit(node.Right);
    }

    public virtual T VisitUnaryOp(UnaryOpNode node) {
        return Visit(node.Operand);
    }

    public virtual T VisitLiteral(LiteralNode node) {
        return default!;
    }

    public virtual T VisitIdentifier(IdentifierNode node) {
        return default!;
    }

    public virtual T VisitFunctionCall(FunctionCallNode node) {
        foreach (var arg in node.Arguments) {
            Visit(arg);
        }
        return default!;
    }

    protected virtual T Visit(AstNode node) {
        return node.Accept(this);
    }
}
