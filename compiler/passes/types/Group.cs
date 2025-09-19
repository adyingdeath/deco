using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Types;

/// <summary>
/// This class groups the type checking and resolution step for clarity and readability.
/// Returns the AST with resolved types.
/// </summary>
public class Group(Scope symbolTable) {
    private readonly Scope _symbolTable = symbolTable;

    public AstNode Visit(AstNode astNode) {
        // Assume symbol tables are already built by Collect_Symbol passes
        var typeResolver = new TypeResolver(_symbolTable);
        var resolvedAst = astNode.Accept(typeResolver);
        if (typeResolver.Errors.Count != 0) {
            Console.WriteLine("Type check errors:");
            foreach (var error in typeResolver.Errors) {
                Console.WriteLine($"  {error}");
            }
        }
        return resolvedAst;
    }
}
