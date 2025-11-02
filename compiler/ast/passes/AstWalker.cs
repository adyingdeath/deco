namespace Deco.Compiler.Ast.Passes;

/// <summary>
/// A utility class for traversing AST trees and setting parent references.
/// </summary>
public static class AstWalker {
    /// <summary>
    /// Sets parent references for all nodes in the AST tree rooted at the given node.
    /// </summary>
    /// <param name="node">The root node of the AST tree.</param>
    public static void SetParents(AstNode node) {
        foreach (var child in node.GetChildren()) {
            child.Parent = node;
            SetParents(child); // Recursive call
        }
    }
}
