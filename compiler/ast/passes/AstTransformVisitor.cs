namespace Deco.Compiler.Ast.Passes;

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
        var newVarDefs = node.VariableDefinitions.Select(v => (VariableDefinitionNode)Visit(v)).ToList();
        var newFunctions = node.Functions.Select(f => (FunctionNode)Visit(f)).ToList();
        return node.With(
            variableDefinitions: newVarDefs,
            functions: newFunctions
        );
    }

    public virtual AstNode VisitFunction(FunctionNode node) {
        var newModifiers = node.Modifiers.Select(m => (ModifierNode)Visit(m)).ToList();
        var newArguments = node.Arguments.Select(a => (ArgumentNode)Visit(a)).ToList();
        var newBody = (BlockNode)Visit(node.Body);
        return node.With(
            modifiers: newModifiers,
            arguments: newArguments,
            body: newBody
        );
    }

    public virtual AstNode VisitModifier(ModifierNode node) {
        var newParameters = node.Parameters.Select(p => (ExpressionNode)Visit(p)).ToList();
        return node.With(parameters: newParameters);
    }

    public virtual AstNode VisitArgument(ArgumentNode node) {
        // Arguments have no children to visit, so return a copy of itself.
        return node.With();
    }

    public virtual AstNode VisitExpressionStatement(ExpressionStatementNode node) {
        var newExpression = (ExpressionNode)Visit(node.Expression);
        return node.With(expression: newExpression);
    }

    public virtual AstNode VisitCommand(CommandNode node) {
        return node; // No children to transform
    }

    public virtual AstNode VisitVariableDefinition(VariableDefinitionNode node) {
        ExpressionNode? newInit = null;
        if (node.InitialValue != null) {
            newInit = (ExpressionNode)Visit(node.InitialValue);
        }
        var newName = (IdentifierNode)Visit(node.Name);
        return node.With(name: newName, initialValue: newInit);
    }

    public virtual AstNode VisitAssignment(AssignmentNode node) {
        var newExpression = (ExpressionNode)Visit(node.Expression);
        return node.With(expression: newExpression);
    }

    public virtual AstNode VisitReturn(ReturnNode node) {
        ExpressionNode? newExpression = null;
        if (node.Expression != null) {
            newExpression = (ExpressionNode)Visit(node.Expression);
        }
        return node.With(expression: newExpression);
    }

    public virtual AstNode VisitIf(IfNode node) {
        var newCondition = (ExpressionNode)Visit(node.Condition);
        var newThenBlock = (BlockNode)Visit(node.ThenBlock);
        BlockNode? newElseBlock = null;
        if (node.ElseBlock != null) {
            newElseBlock = (BlockNode)Visit(node.ElseBlock);
        }
        return node.With(
            condition: newCondition,
            thenBlock: newThenBlock,
            elseBlock: newElseBlock
        );
    }

    public virtual AstNode VisitWhile(WhileNode node) {
        var newCondition = (ExpressionNode)Visit(node.Condition);
        var newBody = (BlockNode)Visit(node.Body);
        return node.With(condition: newCondition, body: newBody);
    }

    public virtual AstNode VisitFor(ForNode node) {
        var newInit =
            node.Initialization == null ?
            null :
            (StatementNode)Visit(node.Initialization);
        var newCondition =
            node.Condition == null
            ? null
            : (ExpressionNode)Visit(node.Condition);
        var newIter =
            node.Iteration == null ?
            null :
            (StatementNode)Visit(node.Iteration);
        var newBody = (BlockNode)Visit(node.Body);
        return node.With(
            initialization: newInit,
            condition: newCondition,
            iteration: newIter,
            body: newBody
        );
    }

    /// <summary>
    /// <para>
    /// The special operation here handles FakeBlockNodes. In its own Visit 
    /// method, a FakeBlockNode is processed like a regular BlockNode. However, 
    /// when processing the statements within a block here, if a StatementNode 
    /// is a FakeBlockNode, it gets flattened. That means the inner statements 
    /// of the FakeBlockNode are lifted into the current block's statements, and
    /// the FakeBlockNode itself is removed. This is primarily to facilitate the
    /// conversion from for-loops to while-loops.
    /// </para>
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public virtual AstNode VisitBlock(BlockNode node) {
        var newStatements = new List<StatementNode>();
        foreach (var statement in node.Statements) {
            var visitedStatement = (StatementNode)Visit(statement);
            if (visitedStatement is FakeBlockNode fakeBlockNode) {
                // If the Node is FakeBlockNode, flaten it
                newStatements.AddRange(fakeBlockNode.Statements);
            } else {
                newStatements.Add(visitedStatement);
            }
        }
        return node.With(statements: newStatements);
    }

    // Treated like normal BlockNode
    public virtual AstNode VisitFakeBlock(FakeBlockNode node) {
        var newStatements = node.Statements.Select(s => (StatementNode)Visit(s)).ToList();
        return node.With(statements: newStatements);
    }

    public virtual AstNode VisitBinaryOp(BinaryOpNode node) {
        var newLeft = (ExpressionNode)Visit(node.Left);
        var newRight = (ExpressionNode)Visit(node.Right);
        return node.With(left: newLeft, right: newRight);
    }

    public virtual AstNode VisitUnaryOp(UnaryOpNode node) {
        var newOperand = (ExpressionNode)Visit(node.Operand);
        return node.With(operand: newOperand);
    }

    public virtual AstNode VisitLiteral(LiteralNode node) {
        return node; // No children to transform
    }

    public virtual AstNode VisitIdentifier(IdentifierNode node) {
        return node; // No children to transform
    }

    public virtual AstNode VisitFunctionCall(FunctionCallNode node) {
        var newArguments = node.Arguments.Select(a => (ExpressionNode)Visit(a)).ToList();
        return node.With(arguments: newArguments);
    }
}
