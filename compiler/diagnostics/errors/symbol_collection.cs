using Deco.Compiler.Types;

namespace Deco.Compiler.Diagnostics.Errors;

public record InternalFunctionNotFoundError(
    string FunctionName,
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.SymbolCollection
) : CompilationError(Line, Column, Severity.Error, Phase) {
    public override string Message =>
        $"Internal compiler error: Function '{FunctionName}' not found in global scope.";
}

public record UndefinedIdentifierError(
    string IdentifierName,
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.TypeChecking
) : CompilationError(Line, Column, Severity.Error, Phase) {
    public override string Message =>
        $"Undefined identifier '{IdentifierName}'.";
}

public record DuplicateSymbolError(
    Symbol ExistingSymbol,
    Symbol NewSymbolName,
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.SymbolCollection
) : CompilationError(Line, Column, Severity.Error, Phase) {
    public override string Message =>
        $"Symbol '{NewSymbolName.Name}' is already defined at line {ExistingSymbol.Line}.";
}