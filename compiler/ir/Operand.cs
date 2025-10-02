using System.Text.RegularExpressions;
using Deco.Types;

namespace Deco.Compiler.IR;

public abstract class Operand {
    public abstract bool IsScoreboard { get; }
}

public partial class ConstantOperand(string value) : Operand {
    public string Value { get; } = value;
    public override bool IsScoreboard => IsScoreboardConstant(Value);
    public override string ToString() => $"Constant({Value})";
    public static bool IsScoreboardConstant(string constant) {
        if (constant.Equals("true") || constant.Equals("false")) {
            return true;
        } else if (IntegerRegex().IsMatch(constant)) {
            // Integer
            return true;
        }
        return false;
    }

    [GeneratedRegex("[0-9]+")]
    private static partial Regex IntegerRegex();
}

/// <summary>
/// Represents variables in IR. In Minecraft datapacks, variables are stored in 
/// scoreboard objectives or storage.
/// The code field represents the unique identifier (usually from Symbol.Code).
/// </summary>
public abstract class VariableOperand(string code) : Operand {
    public string Code { get; } = code;
    public static Operand Create(Symbol symbol) {
        if (TypeUtils.IsScoreboard(symbol.Type)) {
            return new ScoreboardOperand(symbol.Code);
        }
        return new StorageOperand(symbol.Code);
    }
}

/// <summary>
/// Variable stored in Minecraft scoreboard.
/// </summary>
public class ScoreboardOperand(string code) : VariableOperand(code) {
    public override bool IsScoreboard => true;
    public override string ToString() => $"Scoreboard({Code})";
}

/// <summary>
/// Variable stored in Minecraft data storage.
/// </summary>
public class StorageOperand(string code) : VariableOperand(code) {
    public override bool IsScoreboard => false;
    public override string ToString() => $"Storage({Code})";
}
