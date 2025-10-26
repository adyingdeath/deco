using Deco.Compiler.IR;

namespace Deco.Compiler.Pack;

public partial class DatapackBuilder {
    public override List<string> VisitBinaryInstruction(BinaryInstruction inst) {
        return [];;
    }
    public override List<string> VisitUnaryInstruction(UnaryInstruction inst) {
        return [];
    }
    public override List<string> VisitAddInstruction(AddInstruction inst) => BuildArithmetic(inst, "+");
    public override List<string> VisitSubtractInstruction(SubtractInstruction inst) => BuildArithmetic(inst, "-");
    public override List<string> VisitMultiplyInstruction(MultiplyInstruction inst) => BuildArithmetic(inst, "*");
    public override List<string> VisitDivideInstruction(DivideInstruction inst) => BuildArithmetic(inst, "/");

    public override List<string> VisitEqualInstruction(EqualInstruction inst) => BuildComparison(inst.Destination, "=", false, inst.Left, inst.Right);
    public override List<string> VisitNotEqualInstruction(NotEqualInstruction inst) => BuildComparison(inst.Destination, "=", true, inst.Left, inst.Right);
    public override List<string> VisitLessThanInstruction(LessThanInstruction inst) => BuildComparison(inst.Destination, "<", false, inst.Left, inst.Right);
    public override List<string> VisitLessThanOrEqualInstruction(LessThanOrEqualInstruction inst) => BuildComparison(inst.Destination, "<=", false, inst.Left, inst.Right);
    public override List<string> VisitGreaterThanInstruction(GreaterThanInstruction inst) => BuildComparison(inst.Destination, ">", false, inst.Left, inst.Right);
    public override List<string> VisitGreaterThanOrEqualInstruction(GreaterThanOrEqualInstruction inst) => BuildComparison(inst.Destination, ">=", false, inst.Left, inst.Right);
    
    public override List<string> VisitLogicalAndInstruction(LogicalAndInstruction inst) {
        // Emulate logical AND with multiplication (1 * 1 = 1, 1 * 0 = 0)
        return BuildArithmetic(inst, "*");
    }

    public override List<string> VisitLogicalOrInstruction(LogicalOrInstruction inst) {
        var commands = BuildArithmetic(inst, "+"); // dest = left + right
        if (inst.Destination is ScoreboardOperand dest) {
            // If dest > 1, set dest to 1. (Clamps the result of 1+1=2 back to 1)
            commands.Add($"execute if score {dest.Code} {_context.Datapack.Id} matches 2.. run scoreboard players set {dest.Code} {_context.Datapack.Id} 1");
        }
        return commands;
    }

    // --- Specific Unary Instruction Visitors ---
    
    public override List<string> VisitNegateInstruction(NegateInstruction inst) {
        if (inst.Destination is not ScoreboardOperand dest) {
            return [$"""# ERROR: Negation destination must be a scoreboard, but got "{inst.Destination}"."""];
        }

        var sOperand = GetScoreboardOperand(inst.Operand, out var setupCommands);
        var commands = setupCommands;

        // dest = 0
        commands.Add($"scoreboard players set {dest.Code} {_context.Datapack.Id} 0");
        // dest -= operand
        commands.Add($"scoreboard players operation {dest.Code} {_context.Datapack.Id} -= {sOperand.Code} {_context.Datapack.Id}");

        return commands;
    }

    public override List<string> VisitLogicalNotInstruction(LogicalNotInstruction inst) {
        if (inst.Destination is not ScoreboardOperand dest) {
            return [$"""# ERROR: Logical NOT destination must be a scoreboard, but got "{inst.Destination}"."""];
        }

        var sOperand = GetScoreboardOperand(inst.Operand, out var setupCommands);
        var commands = setupCommands;

        // dest = 1
        commands.Add($"scoreboard players set {dest.Code} {_context.Datapack.Id} 1");
        // dest -= operand (1-1=0, 1-0=1)
        commands.Add($"scoreboard players operation {dest.Code} {_context.Datapack.Id} -= {sOperand.Code} {_context.Datapack.Id}");

        return commands;
    }
}
