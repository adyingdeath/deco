using Deco.Compiler.IR;
using Deco.Compiler.Types;

namespace Deco.Compiler.Lib.Api;

/// <summary>
/// Represents a value in library functions, abstracting away IR implementation details.
/// Provides methods to access value properties and format them for Minecraft commands.
/// </summary>
public readonly struct LibraryValue(Operand operand, IType type, string datapackId = "") {
    private readonly Operand _operand = operand;
    private readonly IType _type = type;
    private readonly string _datapackId = datapackId;

    /// <summary>
    /// Gets whether this value is a compile-time constant.
    /// </summary>
    public bool IsConstant => _operand is ConstantOperand;

    /// <summary>
    /// Gets whether this value is a runtime variable.
    /// </summary>
    public bool IsVariable => _operand is VariableOperand;

    /// <summary>
    /// Gets whether this variable is stored in scoreboard.
    /// </summary>
    public bool IsScoreboard => _operand is ScoreboardOperand;

    /// <summary>
    /// Gets whether this variable is stored in data storage.
    /// </summary>
    public bool IsStorage => _operand is StorageOperand;

    /// <summary>
    /// Gets the value as an integer if it's a constant.
    /// </summary>
    public int AsInt {
        get {
            if (_operand is ConstantOperand c && int.TryParse(c.Value, out var result)) {
                return result;
            }
            throw new InvalidOperationException("Value is not a constant integer");
        }
    }

    /// <summary>
    /// Gets the value as a float if it's a constant.
    /// </summary>
    public float AsFloat {
        get {
            if (_operand is ConstantOperand c && float.TryParse(c.Value, out var result)) {
                return result;
            }
            throw new InvalidOperationException("Value is not a constant float");
        }
    }

    /// <summary>
    /// Gets the value as a string if it's a constant.
    /// </summary>
    public string AsString {
        get {
            if (_operand is ConstantOperand c) {
                return c.Value;
            }
            throw new InvalidOperationException("Value is not a constant string");
        }
    }

    /// <summary>
    /// Gets the value as a boolean if it's a constant.
    /// </summary>
    public bool AsBool {
        get {
            if (_operand is ConstantOperand c) {
                if (c.Value.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
                if (c.Value.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
                if (int.TryParse(c.Value, out var i)) return i != 0;
            }
            throw new InvalidOperationException("Value is not a constant boolean");
        }
    }

    /// <summary>
    /// Gets the variable ID for scoreboard/storage variables.
    /// </summary>
    public string VariableId {
        get {
            if (_operand is VariableOperand v) {
                return v.Code;
            }
            throw new InvalidOperationException("Value is not a variable");
        }
    }

    /// <summary>
    /// Gets the type of this value.
    /// </summary>
    public IType Type => _type;

    /// <summary>
    /// Gets the objective name for scoreboard variables (e.g., "my_datapack.int").
    /// Returns empty string for constants or storage variables.
    /// </summary>
    public string Objective {
        get {
            if (_operand is ScoreboardOperand && _datapackId.Length > 0) {
                return _datapackId;
            }
            return "";
        }
    }

    /// <summary>
    /// Gets the storage path for storage variables (e.g., "my_datapack.float").
    /// Returns empty string for constants or scoreboard variables.
    /// </summary>
    public string Storage {
        get {
            if (_operand is StorageOperand && _datapackId.Length > 0) {
                return _datapackId;
            }
            return "";
        }
    }
}
