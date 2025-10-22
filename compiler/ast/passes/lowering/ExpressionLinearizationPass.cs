using Deco.Types;

namespace Deco.Compiler.Ast.Passes.Lowering;

public class ExpressionLinearizationPass : AstTransformVisitor {
    private readonly List<StatementNode> CurrentStatements = [];

    public override AstNode VisitBinaryOp(BinaryOpNode node) {
        var newLeft = (ExpressionNode)Visit(node.Left);
        var newRight = (ExpressionNode)Visit(node.Right);

        /* If the newLeft or newRight is not a smallest unit like identifier or
        literal, we need to create a new temporary variable and assign the newLeft
        expression to the new temporary variable.
        Example:

        int a = a + b + c;
        
        Will become:
        
        int temp1 = a + b;
        int a = temp1 + c
        
        Because the newLeft is "a + b" which is not a smallest unit so we create
        the temporary variable and assign the expression to it.
        We don't know the type of temporary variables so an UnknownType is used
        here. */

        if (!(newLeft is IdentifierNode || newLeft is LiteralNode)) {
            var tempName = Compiler.variableCodeGen.Next();
            var name = new IdentifierNode(
                TypeUtils.UnknownType, tempName, newLeft.Line, newLeft.Column
            );
            CurrentStatements.Add(
                new VariableDefinitionNode(name, newLeft)
            );
            newLeft = new IdentifierNode(TypeUtils.UnknownType, tempName);
        }

        if (!(newRight is IdentifierNode || newRight is LiteralNode)) {
            var tempName = Compiler.variableCodeGen.Next();
            var name = new IdentifierNode(
                TypeUtils.UnknownType, tempName, newRight.Line, newRight.Column
            );
            CurrentStatements.Add(
                new VariableDefinitionNode(name, newRight)
            );
            newRight = new IdentifierNode(TypeUtils.UnknownType, tempName);
        }

        return node.With(left: newLeft, right: newRight);
    }

    public override AstNode VisitVariableDefinition(VariableDefinitionNode node) {
        CurrentStatements.Clear();
        ExpressionNode? newInit = null;
        if (node.InitialValue != null) {
            newInit = (ExpressionNode)Visit(node.InitialValue);
        }
        var newName = (IdentifierNode)Visit(node.Name);
        var definition = new VariableDefinitionNode(
            newName, newInit, node.Line, node.Column
        );
        if (CurrentStatements.Count > 0) {
            var newStatements = CurrentStatements.Append(definition).ToList();
            return new FakeBlockNode(newStatements, node.Line, node.Column);
        } else {
            return definition;
        }
    }

    public override AstNode VisitExpressionStatement(ExpressionStatementNode node) {
        CurrentStatements.Clear();
        var newExpression = (ExpressionNode)Visit(node.Expression);
        var statement = new ExpressionStatementNode(newExpression, node.Line, node.Column);
        if (CurrentStatements.Count > 0) {
            var newStatements = CurrentStatements.Append(statement).ToList();
            return new FakeBlockNode(newStatements, node.Line, node.Column);
        } else {
            return statement;
        }
    }

    public override AstNode VisitAssignment(AssignmentNode node) {
        CurrentStatements.Clear();
        var newExpression = (ExpressionNode)Visit(node.Expression);
        var statement = new AssignmentNode(node.Variable, newExpression, node.Line, node.Column);
        if (CurrentStatements.Count > 0) {
            var newStatements = CurrentStatements.Append(statement).ToList();
            return new FakeBlockNode(newStatements, node.Line, node.Column);
        } else {
            return statement;
        }
    }
}
