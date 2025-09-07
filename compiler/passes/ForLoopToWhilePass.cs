using Deco.Ast;

namespace Deco.Compiler.Passes;

/// <summary>
/// <para>A transformation pass that converts for-loops into while-loops wrapped in a block.
/// This transforms for (init; condition; iter) { body } into:</para>
/// <code>
/// {
///   init;
///   while (condition) {
///     body;
///     iter;
///   }
/// }
/// </code>
/// <para>However, the outer block is <see cref="Deco.Ast.FakeBlockNode"/>, which will be
/// flattened during transformation from for-loop to while-loop. So it will end up like:</para>
/// <code>
/// init;
/// while (condition) {
///   body;
///   iter;
/// }
/// </code>
/// </summary>
public class ForLoopToWhilePass : AstTransformVisitor {
    public override AstNode VisitFor(ForNode node) {
        // First, visit all child nodes to apply transformations recursively
        var newCondition = (ExpressionNode)Visit(node.Condition);
        var newBody = (BlockNode)Visit(node.Body);

        // Now transform the for-loop into a block containing initialization and while-loop
        List<StatementNode> blockStatements = [];

        // Add initialization statement if present
        if (node.Initialization != null) {
            blockStatements.Add(node.Initialization);
        }

        // Create new body for the while-loop: original body + iteration statement
        List<StatementNode> whileBodyStatements = [.. newBody.Statements];
        if (node.Iteration != null) {
            whileBodyStatements.Add(node.Iteration);
        }

        var whileBody = new BlockNode(whileBodyStatements, newBody.Line, newBody.Column);
        var whileNode = new WhileNode(newCondition, whileBody, node.Line, node.Column);
        blockStatements.Add(whileNode);

        // Return the transformed block. Here we use FakeBlockNode, which will be flattened during
        // transformation from for-loop to while-loop
        return new FakeBlockNode(blockStatements, node.Line, node.Column);
    }
}
