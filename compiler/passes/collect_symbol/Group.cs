using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.Passes.Collect_Symbol;

/// <summary>
/// This class is used to group those steps for building and checking symbol tables for
/// clarity and readability.
/// </summary>
/// <param name="symbolTable"></param>
public class Group(Scope symbolTable) {
    public void Visit(AstNode astNode) {
        var gstBuilder = new GlobalSymbolTableBuilder(symbolTable);
        var sstBuilder = new ScopedSymbolTableBuilder(symbolTable);
        gstBuilder.VisitProgram((ProgramNode)astNode);
        sstBuilder.VisitProgram((ProgramNode)astNode);
        if (gstBuilder.Errors.Count != 0) {
            Console.WriteLine("Global symbol table errors:");
            foreach (var error in gstBuilder.Errors) {
                Console.WriteLine($"  {error}");
            }
        }
        if (sstBuilder.Errors.Count != 0) {
            Console.WriteLine("Function scope symbol table errors:");
            foreach (var error in sstBuilder.Errors) {
                Console.WriteLine($"  {error}");
            }
        }

        var usageChecker = new IdentifierUsageChecker(symbolTable);
        usageChecker.Visit(astNode);
        if (usageChecker.Errors.Count != 0) {
            Console.WriteLine("Identifier usage errors:");
            foreach (var error in usageChecker.Errors) {
                Console.WriteLine($"  {error}");
            }
        }
    }
}
