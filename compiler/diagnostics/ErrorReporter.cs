using System.Collections.ObjectModel;

namespace Deco.Compiler.Diagnostics;

public class ErrorReporter {
    private readonly List<CompilationError> _errors = [];
    public ReadOnlyCollection<CompilationError> Errors => _errors.AsReadOnly();
    public bool HasErrors => _errors.Any(e => e.Severity == Severity.Error);
    public bool HasWarnings => _errors.Any(e => e.Severity == Severity.Warning);

    public void Report(CompilationError error) {
        _errors.Add(error);
    }

    public void PrintAll() {
        // Sort by filename, line and column so the information is cleaner.
        var sortedErrors = _errors.OrderBy(e => e.Line).ThenBy(e => e.Column);

        foreach (var error in sortedErrors) {
            switch (error.Severity) {
                case Severity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Error");
                    break;
                case Severity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Warning");
                    break;
                case Severity.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("Info");
                    break;
            }

            Console.ResetColor();
            Console.WriteLine($" [{error.Phase}] ({error.Line}:{error.Column}): {error.Message}");
        }
    }
}