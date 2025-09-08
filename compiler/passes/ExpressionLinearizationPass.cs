using Deco.Ast;

namespace Deco.Compiler.Passes;

public class ExpressionLinearizationPass : AstTransformVisitor {
    private List<StatementNode> CurrentStatements = new();

    public override AstNode VisitBinaryOp(BinaryOpNode node) {
        var newLeft = (ExpressionNode)Visit(node.Left);
        var newRight = (ExpressionNode)Visit(node.Right);

        if (!(newLeft is IdentifierNode || newLeft is LiteralNode)) {
            var tempName = Base36Counter.Next();
            CurrentStatements.Add(new VariableDefinitionNode(
                "[temp]", tempName, newLeft,
                node.Line, node.Column
            ));
            newLeft = new IdentifierNode(tempName);
        }

        if (!(newRight is IdentifierNode || newRight is LiteralNode)) {
            var tempName = Base36Counter.Next();
            CurrentStatements.Add(new VariableDefinitionNode(
                "[temp]", tempName, newRight,
                node.Line, node.Column
            ));
            newRight = new IdentifierNode(tempName);
        }

        return new BinaryOpNode(newLeft, node.Operator, newRight, node.Line, node.Column);
    }

    public override AstNode VisitVariableDefinition(VariableDefinitionNode node) {
        CurrentStatements.Clear();
        ExpressionNode? newInit = null;
        if (node.InitialValue != null) {
            newInit = (ExpressionNode)Visit(node.InitialValue);
        }
        var definition = new VariableDefinitionNode(node.Type, node.Name, newInit, node.Line, node.Column);
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
