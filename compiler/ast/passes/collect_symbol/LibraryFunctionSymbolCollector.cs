using Deco.Compiler.Lib;
using Deco.Types;

namespace Deco.Compiler.Ast.Passes.Collect_Symbol;

public class LibraryFunctionSymbolCollector {
    public static void Build(Scope global, List<DecoFunction> functions) {
        foreach (var func in functions) {
            HandleDecoFunction(global, func);
        }
    }

    public static void HandleDecoFunction(Scope global, DecoFunction function) {
        Scope scope = global.CreateChild($"function {function.Name}");
        var returnType = new UnresolvedType(function.ReturnType);

        // ----- Parameters -----
        List<IType> parameterTypes = [];
        List<Symbol> parameterSymbols = [];
        foreach (var param in function.Parameters) {
            var paramType = new UnresolvedType(param.Type);
            parameterTypes.Add(paramType);
            var paramSymbol = new Symbol(
                param.Name,
                Compiler.variableCodeGen.Next(),
                paramType,
                SymbolKind.Parameter,
                0,
                0
            );
            scope.AddSymbol(paramSymbol);
            parameterSymbols.Add(paramSymbol);
        }

        // ----- Return Value -----
        var returnSymbol = new Symbol(
            $"{function.Name}#return",
            Compiler.variableCodeGen.Next(),
            returnType,
            SymbolKind.Variable,
            0,
            0
        );
        scope.AddSymbol(returnSymbol);

        // ----- Function -----
        var functionType = new FunctionType(returnType, parameterTypes);
        var functionSymbol = new LibraryFunctionSymbol(
            function.Name,
            "###", // Library functions don't have entities, so we don't need code.
            functionType,
            parameterSymbols,
            Symbol.Uninitialized,
            function,
            0,
            0
        );
        global.AddSymbol(functionSymbol);
    }
}