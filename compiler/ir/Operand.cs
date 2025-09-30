using Deco.Types;

namespace Deco.Compiler.IR;

public abstract class Operand { }

public class ConstantOperand(string value) : Operand {
    public string Value { get; } = value;
    public override string ToString() => $"Constant({Value})";
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
    public override string ToString() => $"Scoreboard({Code})";
}

/// <summary>
/// Variable stored in Minecraft data storage.
/// </summary>
public class StorageOperand(string code) : VariableOperand(code) {
    public override string ToString() => $"Storage({Code})";
}
