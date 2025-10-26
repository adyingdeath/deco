using Deco.Compiler.Types;
using Deco.Compiler.Lib;
using Deco.Compiler.Types;

namespace Deco.Compiler.Ast.Passes;

public class LibraryFunctionSymbolCollector {
    public static void Build(
        CompilationContext context, Scope global, List<DecoFunction> functions
    ) {
        foreach (var func in functions) {
            HandleDecoFunction(context, global, func);
        }
    }

    public static void HandleDecoFunction(CompilationContext context, Scope global, DecoFunction function) {
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
                context.VariableCodeGen.Next(),
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
            context.VariableCodeGen.Next(),
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