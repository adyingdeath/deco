using Deco.Compiler.IR;
using Deco.Compiler.Types;

namespace Deco.Compiler.Lib.Api;

/// <summary>
/// Provides context for library functions to generate Minecraft commands.
/// Library functions use this API to emit commands and create values.
/// </summary>
public class LibraryContext(CompilationContext compilerContext, List<IRInstruction> instructions) {
    public CompilationContext Compiler => compilerContext;
    private readonly List<IRInstruction> _instructions = instructions;

    /// <summary>
    /// Emits a Minecraft command.
    /// </summary>
    public void EmitCommand(string command) {
        _instructions.Add(new CommandInstruction(command));
    }

    /// <summary>
    /// Creates a temporary variable for the specified type name.
    /// </summary>
    public LibraryValue CreateTemp(string typeName) {
        var type = TypeUtils.ParseType(typeName);
        var operand = OperandUtils.CreateTemporaryForType(type, Compiler.VariableCodeGen.Next());
        return new LibraryValue(operand, type, DatapackId);
    }

    /// <summary>
    /// Creates a temporary variable based on an existing IType.
    /// </summary>
    public LibraryValue CreateTemp(IType type) {
        var operand = OperandUtils.CreateTemporaryForType(type, Compiler.VariableCodeGen.Next());
        return new LibraryValue(operand, type, DatapackId);
    }

    /// <summary>
    /// Creates a constant value from an integer.
    /// </summary>
    public LibraryValue CreateConstant(int value) {
        return new LibraryValue(new ConstantOperand(value.ToString()), TypeUtils.IntType);
    }

    /// <summary>
    /// Creates a constant value from a float.
    /// </summary>
    public LibraryValue CreateConstant(float value) {
        return new LibraryValue(new ConstantOperand(value.ToString()), TypeUtils.FloatType);
    }

    /// <summary>
    /// Creates a constant value from a string.
    /// </summary>
    public LibraryValue CreateConstant(string value) {
        return new LibraryValue(new ConstantOperand(value), TypeUtils.StringType);
    }

    /// <summary>
    /// Creates a constant value from a boolean.
    /// </summary>
    public LibraryValue CreateConstant(bool value) {
        return new LibraryValue(new ConstantOperand(value.ToString().ToLower()), TypeUtils.BoolType);
    }

    /// <summary>
    /// Gets the datapack ID for use in commands.
    /// </summary>
    public string DatapackId => Compiler.Datapack.Id;
}

/// <summary>
/// Optional interface for plugin initialization logic.
/// </summary>
public interface IDecoPlugin {
    void Initialize(CompilationContext context);
}
