using Deco.Compiler.IR;

namespace Deco.Compiler.Pack;

public partial class DatapackBuilder {
    // Helper Methods for Calculation Command Generation

    /// <summary>
    /// Generates commands for an arithmetic operation (e.g., +, -, *, /).
    /// Assumes the destination is a scoreboard operand. The pattern is:
    /// 1. Move the left operand to the destination.
    /// 2. Apply the operation with the right operand to the destination.
    /// </summary>
    private List<string> BuildArithmetic(BinaryInstruction inst, string op) {
        var commands = new List<string>();

        if (inst.Destination is not ScoreboardOperand dest) {
            return [$"""# ERROR: Arithmetic destination must be a scoreboard operand, but got "{inst.Destination}"."""];
        }

        // 1. Move left operand to destination
        commands.AddRange(VisitMoveInstruction(new MoveInstruction(inst.Left, inst.Destination)));

        // 2. Apply `dest op= right`
        // `scoreboard players add/remove` can use a constant directly.
        if (inst.Right is ConstantOperand rightConst && op is "+" or "-") {
            string command = op == "+" ? "add" : "remove";
            string value = op == "+" ? rightConst.Value : rightConst.Value.TrimStart('-');
            commands.Add($"scoreboard players {command} {dest.Code} {_context.Datapack.Id} {value}");
        } else {
            // For other operations (*, /) or non-constant right operands,
            // we must use `scoreboard players operation`, which requires both operands to be scoreboards.
            var sRight = GetScoreboardOperand(inst.Right, commands);
            commands.Add($"scoreboard players operation {dest.Code} {_context.Datapack.Id} {op}= {sRight.Code} {_context.Datapack.Id}");
        }

        return commands;
    }
    
    /// <summary>
    /// Generates commands for a comparison operation (e.g., ==, !=, <).
    /// The result (0 for false, 1 for true) is stored in the destination scoreboard operand.
    /// </summary>
    private List<string> BuildComparison(Operand destOp, string op, bool invert, Operand left, Operand right) {
        if (destOp is not ScoreboardOperand dest) {
            return [$"""# ERROR: Comparison destination must be a scoreboard operand, but got "{destOp}"."""];
        }

        List<string> commands = [
            // Default to false (0)
            $"scoreboard players set {dest.Code} {_context.Datapack.Id} 0"
        ];

        string condition;
        string executeVerb = invert ? "unless" : "if";

        // Use `matches` for equality checks with a constant, as it's efficient.
        bool useMatches = (op == "=") && (left is ScoreboardOperand && right is ConstantOperand || left is ConstantOperand && right is ScoreboardOperand);

        if (useMatches) {
            var sOp = left is ScoreboardOperand sLeft ? sLeft : (ScoreboardOperand)right;
            var cOp = left is ConstantOperand cLeft ? cLeft : (ConstantOperand)right;
            condition = $"score {sOp.Code} {_context.Datapack.Id} matches {cOp.Value}";
        } else {
            // For other comparisons (<, >, etc.), both operands must be scoreboards.
            var sLeft = GetScoreboardOperand(left, commands);
            var sRight = GetScoreboardOperand(right, commands);
            condition = $"score {sLeft.Code} {_context.Datapack.Id} {op} {sRight.Code} {_context.Datapack.Id}";
        }

        // Set to true (1) if the condition is met.
        commands.Add($"execute {executeVerb} {condition} run scoreboard players set {dest.Code} {_context.Datapack.Id} 1");
        return commands;
    }

    /// <summary>
    /// Ensures an operand is a ScoreboardOperand. If it's not, it generates commands
    /// to move its value into a temporary scoreboard variable and returns that variable.
    /// </summary>
    private ScoreboardOperand GetScoreboardOperand(Operand op, List<string> commands) {
        var sOp = GetScoreboardOperand(op, out var setupCommands);
        commands.AddRange(setupCommands);
        return sOp;
    }
    
    private ScoreboardOperand GetScoreboardOperand(Operand op, out List<string> commands) {
        commands = [];
        if (op is ScoreboardOperand sOp) {
            return sOp;
        }

        var temp = new ScoreboardOperand(_context.VariableCodeGen.Next());
        commands.AddRange(VisitMoveInstruction(new MoveInstruction(op, temp)));
        return temp;
    }

    public string GenOperand(Operand? operand) {
        return operand switch {
            ConstantOperand constant => constant.Value,
            VariableOperand variable => variable.Code,
            _ => ""
        };
    }
}
