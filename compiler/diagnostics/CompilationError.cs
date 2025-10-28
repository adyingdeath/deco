using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Deco.Compiler.Diagnostics;

public enum Severity {
    Info,
    Warning,
    Error
}

public enum CompilationPhase {
    General,
    Preprocessing,
    Parsing,
    SymbolCollection,
    TypeChecking,
    IRGeneration,
    DataPackGeneration
}

public abstract record CompilationError(
    int Line, int Column, Severity Severity, CompilationPhase Phase
) {
    // Every error classes takes care of the message themselves.
    public abstract string Message { get; }
}
