using Deco.Compiler.IR;

namespace Deco.Compiler.Pack;

/// <summary>
/// Builds a Datapack from an IrProgram.
/// </summary>
public partial class DatapackBuilder(CompilationContext context) : IRVisitor<List<string>> {
    private readonly CompilationContext _context = context;

    public override List<string> VisitProgram(IrProgram program) {
        foreach (var func in program.Functions) {
            List<string> commands = [];
            foreach (var inst in func.Instructions) {
                commands.AddRange(inst.Accept(this));
            }

            var functionResource = new Function(commands).SetLocation(
                new ResourceLocation(_context.Datapack.Namespace, func.Name)
            );
            _context.Datapack.Functions.Add(functionResource);

            // Handle global load tag
            if (func.Name.Equals("global")) {
                var load = _context.Datapack.Tags.Find((tag) => (
                    tag.Type == TagType.Function
                    && tag.Location.Equals(new ResourceLocation("minecraft", "load"))
                ));
                load?.Entries.Add(functionResource.Location.ToString());
            }
        }
        return [];
    }

    public override List<string> VisitCallInstruction(CallInstruction inst) {
        var location = new ResourceLocation(_context.Datapack.Namespace, inst.TargetFunction);
        // "run function" can store result if needed, but for void calls just run it
        return [$"function {location}"];
    }

    public override List<string> VisitCallIfInstruction(CallIfInstruction inst) {
        var location = new ResourceLocation(_context.Datapack.Namespace, inst.TargetFunction);
        string verb = inst.IsUnless ? "unless" : "if";

        // Optimize for Constant operands in condition
        /* Here we only handle the case of Equal, because conditional expressions
        like if(a < 1) have been evaluated to something like:
        temp = a < 1
        if(temp == 1)
        so there is no need to handle cases other than Equal*/
        if (inst.Condition.Right is ConstantOperand constRight) {
            if (inst.Condition.Left is ScoreboardOperand sLeft) {
                return [$"execute {verb} score {sLeft.Code} {_context.Datapack.Id} matches {constRight.Value} run function {location}"];
            }
        }

        // Generic fallback for variable comparisons
        if (inst.Condition.Left is ScoreboardOperand sbLeft && inst.Condition.Right is ScoreboardOperand sbRight) {
            string op = inst.Condition.Type switch {
                ConditionType.Equal => "=",
                ConditionType.Greater => ">",
                ConditionType.GreaterEqual => ">=",
                ConditionType.Less => "<",
                ConditionType.LessEqual => "<=",
                _ => "="
            };
            return [$"execute {verb} score {sbLeft.Code} {_context.Datapack.Id} {op} {sbRight.Code} {_context.Datapack.Id} run function {location}"];
        }

        return [$"# Unsupported condition type for CallIf: {inst.Condition}"];
    }

    public override List<string> VisitReturnInstruction(ReturnInstruction inst) {
        if (inst.Value == null) return ["return 1"];
        return [$"return {GenOperand(inst.Value)}"];
    }

    public override List<string> VisitReturnIfInstruction(ReturnIfInstruction inst) {
        string returnValue = inst.Value == null ? "1" : GenOperand(inst.Value);
        string runReturn = $"run return {returnValue}";

        // Optimize: variable vs constant (use 'matches')
        // Example: if (a == 1) return ...
        if (inst.Condition.Right is ConstantOperand constRight && inst.Condition.Left is ScoreboardOperand sLeft) {
            if (inst.Condition.Type == ConditionType.Equal) {
                return [$"execute if score {sLeft.Code} {_context.Datapack.Id} matches {constRight.Value} {runReturn}"];
            }
        }
        if (inst.Condition.Left is ConstantOperand constLeft && inst.Condition.Right is ScoreboardOperand sRight) {
            if (inst.Condition.Type == ConditionType.Equal) {
                return [$"execute if score {sRight.Code} {_context.Datapack.Id} matches {constLeft.Value} {runReturn}"];
            }
        }

        // Generic: variable vs variable (use =, <, >, <=, >=)
        // Also handles variable vs constant if constant is moved to a temp variable
        if (inst.Condition.Left is ScoreboardOperand sbLeft && inst.Condition.Right is ScoreboardOperand sbRight) {
            string op = inst.Condition.Type switch {
                ConditionType.Equal => "=",
                ConditionType.Greater => ">",
                ConditionType.GreaterEqual => ">=",
                ConditionType.Less => "<",
                ConditionType.LessEqual => "<=",
                _ => "="
            };

            return [$"execute if score {sbLeft.Code} {_context.Datapack.Id} {op} {sbRight.Code} {_context.Datapack.Id} {runReturn}"];
        }

        // Unsupported cases (e.g. direct Storage comparison)
        return [$"# ERROR: Unsupported condition operands in ReturnIf: {inst.Condition}"];
    }

    public override List<string> VisitCommandInstruction(CommandInstruction inst) {
        return [inst.Command];
    }

    public override List<string> VisitMoveInstruction(MoveInstruction inst) {
        return (inst.Source, inst.Destination) switch {
            // Destination cannot be a ConstantOperand in a move operation.
            (_, ConstantOperand dest) =>
                [$"# ERROR: Destination cannot be a ConstantOperand: {dest.Value}"],

            (ConstantOperand source, ScoreboardOperand dest) =>
                [$"scoreboard players set {dest.Code} {_context.Datapack.Id} {source.GetValueInGame()}"],

            (ScoreboardOperand source, ScoreboardOperand dest) =>
                [$"scoreboard players operation {dest.Code} {_context.Datapack.Id} = {source.Code} {_context.Datapack.Id}"],

            (StorageOperand source, ScoreboardOperand dest) =>
                [$"execute store result score {dest.Code} {_context.Datapack.Id} run data get storage {_context.Datapack.Id} {source.Code}"],

            (ConstantOperand source, StorageOperand dest) =>
                [$"data modify storage {_context.Datapack.Id} {dest.Code} set value {source.GetValueInGame()}"],

            (ScoreboardOperand source, StorageOperand dest) =>
                [$"execute store result storage {_context.Datapack.Id} {dest.Code} float 1.0 run scoreboard players get {source.Code} {_context.Datapack.Id}"],

            (StorageOperand source, StorageOperand dest) =>
                [$"data modify storage {_context.Datapack.Id} {dest.Code} set from storage {_context.Datapack.Id} {source.Code}"],

            _ => []
        };
    }

    public override List<string> VisitPushInstruction(PushInstruction inst) {
        if (inst.Operand is ScoreboardOperand scoreboard) {
            return [
                $"data modify storage minecraft:{_context.Datapack.Id} {scoreboard.StackName} prepend value 0",
                $"execute store result storage minecraft:{_context.Datapack.Id} {scoreboard.StackName}[0] int 1 run scoreboard players get {scoreboard.Code} {_context.Datapack.Id}"
            ];
        }
        return [$"data modify storage minecraft:{_context.Datapack.Id} {inst.Operand.StackName} prepend from storage minecraft:{_context.Datapack.Id} {inst.Operand.Code}"];
    }

    public override List<string> VisitPopInstruction(PopInstruction inst) {
        if (inst.Operand is ScoreboardOperand scoreboard) {
            return [
                $"execute store result score {scoreboard.Code} {_context.Datapack.Id} run data get storage minecraft:{_context.Datapack.Id} {inst.Operand.StackName}[0] 1",
                $"data remove storage minecraft:{_context.Datapack.Id} {scoreboard.StackName}[0]"
            ];
        }
        return [
            // Pop the value to the Operand.
            $"data modify storage minecraft:{_context.Datapack.Id} {inst.Operand.Code} set from storage minecraft:{_context.Datapack.Id} {inst.Operand.StackName}[0]",
            // Remove the element we've just poped.
            $"data remove storage minecraft:{_context.Datapack.Id} {inst.Operand.StackName}[0]",
        ];
    }
}