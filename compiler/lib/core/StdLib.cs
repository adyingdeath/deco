using Deco.Compiler.IR;
using Deco.Compiler.Lib.Api;

namespace Deco.Compiler.Lib.Core;

[DecoLibrary("std")]
public class StdLib {
    [DecoFunction("print", ReturnType = "void")]
    public static void Print(
        LibraryContext ctx,
        [DecoArgument("int")] Operand value
    ) {
        if (value is ConstantOperand c) {
            // Optimization: Constant print
            ctx.Emit(new CommandInstruction($"tellraw @a \"{c.Value}\""));
        } else if (value is ScoreboardOperand s) {
            // Dynamic print
            ctx.Emit(new CommandInstruction(
                $"tellraw @a {{\"score\":{{\"name\":\"{s.Code}\",\"objective\":\"{ctx.Compiler.Datapack.Id}\"}}}}"
            ));
        } else {
            // Fallback for other operand types if necessary
            ctx.Emit(new CommandInstruction($"# Print not supported for {value.GetType().Name}"));
        }
    }
}