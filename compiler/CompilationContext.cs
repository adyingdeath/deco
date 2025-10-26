using Deco.Compiler.Pack;

namespace Deco.Compiler;

public class CompilationContext(Datapack datapack) {
    public Datapack Datapack { get; } = datapack;
    public Base36Counter FunctionCodeGen { get; } = new();
    public Base36Counter VariableCodeGen { get; } = new();
}