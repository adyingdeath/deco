using Deco.Compiler.Ast;

namespace Deco.Compiler.Ast.Passes;

/// <summary>
/// A pass that assigns parent references to all AST nodes except the root.
/// This visitor traverses the AST tree and sets the Parent property on each child node.
/// </summary>
public class FindFatherPass : IAstVisitor<AstNode> {
    /// <summary>
    /// Visits the given AST node and its subtree, assigning parent references.
    /// </summary>
    /// <param name="node">The AST node to visit.</param>
    /// <returns>The same node with parent references set.</returns>
    public AstNode Visit(AstNode node) {
        return node.Accept(this);
    }

    public AstNode VisitProgram(ProgramNode node) {
        foreach (var variableDefinition in node.VariableDefinitions) {
            variableDefinition.Parent = node;
            Visit(variableDefinition);
        }
        foreach (var function in node.Functions) {
            function.Parent = node;
            Visit(function);
        }
        return node;
    }

    public AstNode VisitFunction(FunctionNode node) {
        foreach (var modifier in node.Modifiers) {
            modifier.Parent = node;
            Visit(modifier);
        }
        node.Name.Parent = node;
        Visit(node.Name);
        foreach (var argument in node.Arguments) {
            argument.Parent = node;
            Visit(argument);
        }
        node.Body.Parent = node;
        Visit(node.Body);
        return node;
    }

    public AstNode VisitModifier(ModifierNode node) {
        node.Name.Parent = node;
        Visit(node.Name);
        foreach (var parameter in node.Parameters) {
            parameter.Parent = node;
            Visit(parameter);
        }
        return node;
    }

    public AstNode VisitArgument(ArgumentNode node) {
        node.Name.Parent = node;
        Visit(node.Name);
        return node;
    }

    public AstNode VisitExpressionStatement(ExpressionStatementNode node) {
        node.Expression.Parent = node;
        Visit(node.Expression);
        return node;
    }

    public AstNode VisitCommand(CommandNode node) {
        // Commands have no children, so nothing to set here
        return node;
    }

    public AstNode VisitVariableDefinition(VariableDefinitionNode node) {
        node.Name.Parent = node;
        Visit(node.Name);
        if (node.InitialValue != null) {
            node.InitialValue.Parent = node;
            Visit(node.InitialValue);
        }
        return node;
    }

    public AstNode VisitAssignment(AssignmentNode node) {
        node.Variable.Parent = node;
        Visit(node.Variable);
        node.Expression.Parent = node;
        Visit(node.Expression);
        return node;
    }

    public AstNode VisitReturn(ReturnNode node) {
        if (node.Expression != null) {
            node.Expression.Parent = node;
            Visit(node.Expression);
        }
        return node;
    }

    public AstNode VisitIf(IfNode node) {
        node.Condition.Parent = node;
        Visit(node.Condition);
        node.ThenBlock.Parent = node;
        Visit(node.ThenBlock);
        if (node.ElseBlock != null) {
            node.ElseBlock.Parent = node;
            Visit(node.ElseBlock);
        }
        return node;
    }

    public AstNode VisitWhile(WhileNode node) {
        node.Condition.Parent = node;
        Visit(node.Condition);
        node.Body.Parent = node;
        Visit(node.Body);
        return node;
    }

    public AstNode VisitFor(ForNode node) {
        if (node.Initialization != null) {
            node.Initialization.Parent = node;
            Visit(node.Initialization);
        }
        if (node.Condition != null) {
            node.Condition.Parent = node;
            Visit(node.Condition);
        }
        if (node.Iteration != null) {
            node.Iteration.Parent = node;
            Visit(node.Iteration);
        }
        node.Body.Parent = node;
        Visit(node.Body);
        return node;
    }

    public AstNode VisitBlock(BlockNode node) {
        foreach (var statement in node.Statements) {
            statement.Parent = node;
            Visit(statement);
        }
        return node;
    }

    public AstNode VisitFakeBlock(FakeBlockNode node) {
        foreach (var statement in node.Statements) {
            statement.Parent = node;
            Visit(statement);
        }
        return node;
    }

    public AstNode VisitBinaryOp(BinaryOpNode node) {
        node.Left.Parent = node;
        Visit(node.Left);
        node.Right.Parent = node;
        Visit(node.Right);
        return node;
    }

    public AstNode VisitUnaryOp(UnaryOpNode node) {
        node.Operand.Parent = node;
        Visit(node.Operand);
        return node;
    }

    public AstNode VisitLiteral(LiteralNode node) {
        // Literals have no children, so nothing to set here
        return node;
    }

    public AstNode VisitIdentifier(IdentifierNode node) {
        // Identifiers have no children, so nothing to set here
        return node;
    }

    public AstNode VisitFunctionCall(FunctionCallNode node) {
        node.Name.Parent = node;
        Visit(node.Name);
        foreach (var argument in node.Arguments) {
            argument.Parent = node;
            Visit(argument);
        }
        return node;
    }
}
