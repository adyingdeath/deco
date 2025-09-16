using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Types;

/// <summary>
/// This class groups the type checking step for clarity and readability.
/// </summary>
public class Group(Deco.Types.Scope symbolTable) {
    private readonly Deco.Types.Scope _symbolTable = symbolTable;

    public void Visit(Deco.Ast.AstNode astNode) {
        // Assume symbol tables are already built by Collect_Symbol passes
        var typeChecker = new TypeChecker(_symbolTable);
        typeChecker.Visit(astNode);
        if (typeChecker.Errors.Count != 0) {
            Console.WriteLine("Type check errors:");
            foreach (var error in typeChecker.Errors) {
                Console.WriteLine($"  {error}");
            }
        }
    }
}
