using Deco.Ast;

namespace Deco.Compiler.Passes;

/// <summary>
/// A base visitor for performing transformations on the AST.
/// This visitor traverses the tree and reconstructs it.
/// Derived classes can override specific Visit methods to transform parts of the tree.
/// </summary>
public abstract class AstTransformVisitor : IAstVisitor<AstNode> {
    public virtual AstNode Visit(AstNode node) {
        return node.Accept(this);
    }

    public virtual AstNode VisitProgram(ProgramNode node) {
        var newFunctions = node.Functions.Select(f => (FunctionNode)Visit(f)).ToList();
        return new ProgramNode(newFunctions, node.Line, node.Column);
    }

    public virtual AstNode VisitFunction(FunctionNode node) {
        var newModifiers = node.Modifiers.Select(m => (ModifierNode)Visit(m)).ToList();
        var newArguments = node.Arguments.Select(a => (ArgumentNode)Visit(a)).ToList();
        var newBody = (BlockNode)Visit(node.Body);
        return new FunctionNode(newModifiers, node.ReturnType, node.Name, newArguments, newBody, node.Line, node.Column);
    }

    public virtual AstNode VisitModifier(ModifierNode node) {
        var newParameters = node.Parameters.Select(p => (ExpressionNode)Visit(p)).ToList();
        return new ModifierNode(node.Name, newParameters, node.Line, node.Column);
    }

    public virtual AstNode VisitArgument(ArgumentNode node) {
        // Arguments have no children to visit, so return a new instance or itself.
        return new ArgumentNode(node.Type, node.Name, node.Line, node.Column);
    }

    public virtual AstNode VisitExpressionStatement(ExpressionStatementNode node) {
        var newExpression = (ExpressionNode)Visit(node.Expression);
        return new ExpressionStatementNode(newExpression, node.Line, node.Column);
    }

    public virtual AstNode VisitCommand(CommandNode node) {
        return node; // No children to transform
    }

    public virtual AstNode VisitVariableDefinition(VariableDefinitionNode node) {
        return node; // No children to transform
    }

    public virtual AstNode VisitAssignment(AssignmentNode node) {
        var newExpression = (ExpressionNode)Visit(node.Expression);
        return new AssignmentNode(node.Variable, newExpression, node.Line, node.Column);
    }

    public virtual AstNode VisitReturn(ReturnNode node) {
        ExpressionNode? newExpression = null;
        if (node.Expression != null) {
            newExpression = (ExpressionNode)Visit(node.Expression);
        }
        return new ReturnNode(newExpression, node.Line, node.Column);
    }

    public virtual AstNode VisitIf(IfNode node) {
        var newCondition = (ExpressionNode)Visit(node.Condition);
        var newThenBlock = (BlockNode)Visit(node.ThenBlock);
        BlockNode? newElseBlock = null;
        if (node.ElseBlock != null) {
            newElseBlock = (BlockNode)Visit(node.ElseBlock);
        }
        return new IfNode(newCondition, newThenBlock, newElseBlock, node.Line, node.Column);
    }

    public virtual AstNode VisitWhile(WhileNode node) {
        var newCondition = (ExpressionNode)Visit(node.Condition);
        var newBody = (BlockNode)Visit(node.Body);
        return new WhileNode(newCondition, newBody, node.Line, node.Column);
    }

    public virtual AstNode VisitFor(ForNode node) {
        var newInit =
            node.Initialization == null ?
            null :
            (StatementNode)Visit(node.Initialization);
        var newCondition = (ExpressionNode)Visit(node.Condition);
        var newIter =
            node.Iteration == null ?
            null :
            (StatementNode)Visit(node.Iteration);
        var newBody = (BlockNode)Visit(node.Body);
        return new ForNode(newInit, newCondition, newIter, newBody, node.Line, node.Column);
    }

    public virtual AstNode VisitBlock(BlockNode node) {
        var newStatements = node.Statements.Select(s => (StatementNode)Visit(s)).ToList();
        return new BlockNode(newStatements, node.Line, node.Column);
    }

    public virtual AstNode VisitBinaryOp(BinaryOpNode node) {
        var newLeft = (ExpressionNode)Visit(node.Left);
        var newRight = (ExpressionNode)Visit(node.Right);
        return new BinaryOpNode(newLeft, node.Operator, newRight, node.Line, node.Column);
    }

    public virtual AstNode VisitUnaryOp(UnaryOpNode node) {
        var newOperand = (ExpressionNode)Visit(node.Operand);
        return new UnaryOpNode(node.Operator, newOperand, node.Line, node.Column);
    }

    public virtual AstNode VisitLiteral(LiteralNode node) {
        return node; // No children to transform
    }

    public virtual AstNode VisitIdentifier(IdentifierNode node) {
        return node; // No children to transform
    }

    public virtual AstNode VisitFunctionCall(FunctionCallNode node) {
        var newArguments = node.Arguments.Select(a => (ExpressionNode)Visit(a)).ToList();
        return new FunctionCallNode(node.Name, newArguments, node.Line, node.Column);
    }
}