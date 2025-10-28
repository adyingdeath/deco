using Deco.Compiler.Ast;
using Deco.Compiler.Types;

namespace Deco.Compiler.Diagnostics.Errors;

public record TypeMismatchError(
    IType ExpectedType,
    IType ActualType,
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.TypeChecking
) : CompilationError(Line, Column, Severity.Error, Phase) {
    public override string Message =>
        $"Type mismatch: expected '{ExpectedType}', but got '{ActualType}'.";
}

public record IllegalReturnStatementError(
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.TypeChecking
) : CompilationError(Line, Column, Severity.Error, Phase) {
    public override string Message =>
        $"Illegal return statement.";
}

public record ArgumentCountMismatchError(
    string FunctionName,
    int ExpectedCount,
    int GotCount,
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.TypeChecking
) : CompilationError(Line, Column, Severity.Error, Phase) {
    public override string Message =>
        $"Function '{FunctionName}' expects {ExpectedCount} arguments, got {GotCount}.";
}

public record NonNumericOperandError(
    BinaryOperator Operator,
    IType LeftOperandType,
    IType RightOperandType,
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.TypeChecking
) : CompilationError(Line, Column, Severity.Error, Phase) {
    private readonly string operatorName = Operator switch {
        BinaryOperator.Add => "+",
        BinaryOperator.Subtract => "-",
        BinaryOperator.Multiply => "*",
        BinaryOperator.Divide => "/",
        _ => ""
    };
    public override string Message =>
        $"Operator '{operatorName}' requires numeric operands, but received types: '{LeftOperandType}' and '{RightOperandType}'.";
}

public record IncompatibleComparisonOperandsError(
    BinaryOperator Operator,
    IType LeftOperandType,
    IType RightOperandType,
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.TypeChecking
) : CompilationError(Line, Column, Severity.Error, Phase) {
    private readonly string operatorName = Operator switch {
        BinaryOperator.Equal => "==",
        BinaryOperator.NotEqual => "!=",
        BinaryOperator.LessThan => "<",
        BinaryOperator.LessThanOrEqual => "<=",
        BinaryOperator.GreaterThan => ">",
        BinaryOperator.GreaterThanOrEqual => ">=",
        _ => ""
    };
    public override string Message =>
        $"Comparison operator '{operatorName}' requires compatible or numeric operands, but received types: '{LeftOperandType}' and '{RightOperandType}', which are incompatible.";
}

public record UnknownBinaryOperatorError(
    int Line,
    int Column,
    CompilationPhase Phase = CompilationPhase.TypeChecking
) : CompilationError(Line, Column, Severity.Error, Phase) {
    public override string Message => "Unknown binary operator";
}
