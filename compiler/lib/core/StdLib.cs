using Deco.Compiler.Lib.Api;

namespace Deco.Compiler.Lib.Core;

[DecoLibrary("std")]
public class StdLib {
    [DecoFunction("print", ReturnType = "void")]
    public static void Print(
        LibraryContext ctx,
        [DecoArgument("int")] LibraryValue value
    ) {
        if (value.IsVariable) {
            if (value.IsScoreboard) {
                ctx.EmitCommand($"tellraw @a {{\"score\":{{\"name\":\"{value.VariableId}\",\"objective\":\"{value.Objective}\"}}}}");
            } else {
                ctx.EmitCommand($"tellraw @a {{\"nbt\":\"{value.VariableId}\",\"storage\":\"{value.Storage}\"}}");
            }
        } else {
            ctx.EmitCommand($"tellraw @a \"{value.AsString}\"");
        }
    }
}