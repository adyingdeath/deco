using System.Text.RegularExpressions;
using Deco.Compiler.Types;
using Deco.Compiler.Types;

namespace Deco.Compiler.IR;

public abstract class Operand {
    public abstract bool IsScoreboard { get; }
    public abstract string StackName { get; }
}

public partial class ConstantOperand(string value) : Operand {
    public string Value { get; } = value;
    public override bool IsScoreboard => OperandUtils.EvaluateConstant(this) is ScoreboardOperand;
    // Since we will not use Push and Pop on constant so this is useless.
    public override string StackName => "";
    public override string ToString() => $"Constant({Value})";
    /// <summary>
    /// Method to calculate its value representation in game. Like if this.Value
    /// is "true", the GetValueInGame will return "1" because in minecraft we
    /// store bool type true as 1 in scoreboard.
    /// </summary>
    /// <returns></returns>
    public string GetValueInGame() {
        if (Value.Equals("true")) {
            return "1";
        } else if (Value.Equals("false")) {
            return "0";
        }
        return Value;
    }
}

/// <summary>
/// Represents variables in IR. In Minecraft datapacks, variables are stored in 
/// scoreboard objectives or storage.
/// The code field represents the unique identifier (usually from Symbol.Code).
/// </summary>
public abstract class VariableOperand(string code) : Operand {
    public string Code { get; } = code;
    public static Operand Create(Symbol symbol) {
        return OperandUtils.ParseVariable(symbol.Type, symbol.Code);
    }
}

/// <summary>
/// Variable stored in Minecraft scoreboard.
/// </summary>
public class ScoreboardOperand(string code) : VariableOperand(code) {
    public override bool IsScoreboard => true;

    public override string StackName => Constants.IntStackName;

    public override string ToString() => $"Scoreboard({Code})";
}

/// <summary>
/// Variable stored in Minecraft data storage.
/// </summary>
public abstract class StorageOperand(string code) : VariableOperand(code) {
    public override bool IsScoreboard => false;
    public override string ToString() => $"Storage({Code})";
}

public class FloatOperand(string code) : StorageOperand(code) {
    public override string StackName => Constants.FloatStackName;
}

public class DoubleOperand(string code) : StorageOperand(code) {
    public override string StackName => Constants.DoubleStackName;
}

public class StringOperand(string code) : StorageOperand(code) {
    public override string StackName => Constants.StringStackName;
}

public partial class OperandUtils {
    public static VariableOperand ParseVariable(IType type, string Code) {
        if (type.Equals(TypeUtils.IntType) || type.Equals(TypeUtils.BoolType)) {
            return new ScoreboardOperand(Code);
        } else if (type.Equals(TypeUtils.FloatType)) {
            return new FloatOperand(Code);
        } else if (type.Equals(TypeUtils.StringType)) {
            return new StringOperand(Code);
        }
        return new DoubleOperand(Code);
    }

    /// <summary>
    /// Creates a temporary variable operand based on a resolved language type (IType).
    /// This is the single source of truth for mapping language types to storage types (Operands).
    /// </summary>
    /// <param name="type">The resolved IType from the AST node.</param>
    /// <param name="code">The unique code for this temporary variable.</param>
    /// <returns>A concrete VariableOperand (e.g., ScoreboardOperand, FloatOperand).</returns>
    public static VariableOperand CreateTemporaryForType(IType type, string code) {
        // The logic here is clear and direct
        if (type.IsStorableInScoreboard) // bool and int
        {
            return new ScoreboardOperand(code);
        }

        if (type.Equals(TypeUtils.FloatType)) {
            return new FloatOperand(code);
        }

        if (type.Equals(TypeUtils.StringType)) {
            return new StringOperand(code);
        }

        // If we reach here, it's an unsupported type for a variable.
        throw new InvalidOperationException($"Cannot create a temporary variable for the type '{type.Name}'. This might be a void, function, or unresolved type.");
    }

    /// <summary>
    /// This function evaluates a constant operand and then return a variable
    /// operand, whose type is coresponding to the constant. If the provided
    /// operand is not a constant operand, leave it as it is.
    /// </summary>
    /// <param name="constant"></param>
    /// <returns></returns>
    public static VariableOperand EvaluateConstant(Operand operand) {
        if (operand is ConstantOperand constant) {
            // Try to evaluate the constant
            if (constant.Equals("true") || constant.Equals("false")) {
                // Boolean, scoreboard
                return new ScoreboardOperand("##");
            } else if (IntegerRegex().IsMatch(constant.Value)) {
                // Integer
                return new ScoreboardOperand("##");
            } else if (FloatRegex().IsMatch(constant.Value)) {
                // Integer
                return new FloatOperand("##");
            }
            return new StringOperand("##");
        }
        return (VariableOperand)operand;
    }

    [GeneratedRegex("[0-9]+")]
    private static partial Regex IntegerRegex();
    [GeneratedRegex("[0-9]*\\.[0-9]+")]
    private static partial Regex FloatRegex();
}