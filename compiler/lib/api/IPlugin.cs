using Deco.Compiler.IR;
using Deco.Compiler.Types;

namespace Deco.Compiler.Lib.Api;

/// <summary>
/// Provides context for library functions to generate IR.
/// </summary>
public class LibraryContext(CompilationContext compilerContext, IRBuilder builder, List<IRInstruction> instructions) {
    public CompilationContext Compiler => compilerContext;
    public IRBuilder Builder => builder;

    /// <summary>
    /// Emits an instruction to the current execution flow.
    /// </summary>
    public void Emit(IRInstruction instruction) {
        instructions.Add(instruction);
    }

    /// <summary>
    /// Creates a temporary variable operand for the specified type name.
    /// </summary>
    public VariableOperand CreateTemp(string typeName) {
        var type = TypeUtils.ParseType(typeName);
        return OperandUtils.CreateTemporaryForType(type, Compiler.VariableCodeGen.Next());
    }

    /// <summary>
    /// Creates a temporary variable operand based on an existing IType.
    /// </summary>
    public VariableOperand CreateTemp(IType type) {
        return OperandUtils.CreateTemporaryForType(type, Compiler.VariableCodeGen.Next());
    }
}

/// <summary>
/// Optional interface for plugin initialization logic.
/// </summary>
public interface IDecoPlugin {
    void Initialize(CompilationContext context);
}