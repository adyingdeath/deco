using System.Text.RegularExpressions;
using Deco.Types;

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

    public override string StackName => "Int";

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
    public override string StackName => "Float";
}

public class DoubleOperand(string code) : StorageOperand(code) {
    public override string StackName => "Double";
}

public class StringOperand(string code) : StorageOperand(code) {
    public override string StackName => "String";
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
    public static VariableOperand ResolveVariable(
        Operand left, Operand right, string Code
    ) {
        return (EvaluateConstant(left), EvaluateConstant(right)) switch {
            (ScoreboardOperand, ScoreboardOperand) => new ScoreboardOperand(Code),
            (ScoreboardOperand, FloatOperand) => new FloatOperand(Code),
            (ScoreboardOperand, DoubleOperand) => new DoubleOperand(Code),
            (ScoreboardOperand, StringOperand) => new StringOperand(Code),

            (FloatOperand, ScoreboardOperand) => new FloatOperand(Code),
            (FloatOperand, FloatOperand) => new FloatOperand(Code),
            (FloatOperand, DoubleOperand) => new DoubleOperand(Code),
            (FloatOperand, StringOperand) => new StringOperand(Code),

            (DoubleOperand, ScoreboardOperand) => new DoubleOperand(Code),
            (DoubleOperand, FloatOperand) => new DoubleOperand(Code),
            (DoubleOperand, DoubleOperand) => new DoubleOperand(Code),
            (DoubleOperand, StringOperand) => new StringOperand(Code),

            (StringOperand, ScoreboardOperand) => new StringOperand(Code),
            (StringOperand, FloatOperand) => new StringOperand(Code),
            (StringOperand, DoubleOperand) => new StringOperand(Code),
            (StringOperand, StringOperand) => new StringOperand(Code),

            _ => new StringOperand(Code),
        };
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