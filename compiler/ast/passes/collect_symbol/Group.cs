using Deco.Compiler.Lib.Core;
using Deco.Types;

namespace Deco.Compiler.Ast.Passes.Collect_Symbol;

/// <summary>
/// This class is used to group those steps for building and checking symbol tables for
/// clarity and readability.
/// </summary>
/// <param name="symbolTable"></param>
public class Group(Scope symbolTable) {
    public void Visit(AstNode astNode) {
        var gstBuilder = new GlobalSymbolTableBuilder(symbolTable);
        var sstBuilder = new ScopedSymbolTableBuilder(symbolTable);
        astNode.Accept(gstBuilder);
        astNode.Accept(sstBuilder);
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

        // Collect symbols for library functions.
        LibraryFunctionSymbolCollector.Build(symbolTable, [new PrintFunction()]);

        var usageChecker = new IdentifierUsageChecker(symbolTable);
        astNode.Accept(usageChecker);
        if (usageChecker.Errors.Count != 0) {
            Console.WriteLine("Identifier usage errors:");
            foreach (var error in usageChecker.Errors) {
                Console.WriteLine($"  {error}");
            }
        }
    }
}
