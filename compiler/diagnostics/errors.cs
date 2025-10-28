namespace Deco.Compiler.Diagnostics;

public record UndefinedIdentifierError(
    string IdentifierName,
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.TypeChecking
) : CompilationError(Line, Column, Severity.Error, Phase) {
    public override string Message =>
        $"Undefined identifier '{IdentifierName}'.";
}