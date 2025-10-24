using Deco.Compiler.IR;

namespace Deco.Compiler.Pack;

/// <summary>
/// Builds a Datapack from a list of IR instructions.
/// </summary>
public partial class DatapackBuilder(Datapack datapack) : IRVisitor<List<string>> {
    private readonly Datapack _datapack = datapack;

    public override List<string> VisitProgram(ProgramInstruction inst) {
        inst.Labels.ForEach((label) => label.Accept(this));
        return [];
    }

    public override List<string> VisitLabelInstruction(LabelInstruction inst) {
        List<string> commands = [];
        foreach (var i in inst.Instructions) {
            commands.AddRange(i.Accept(this));
        }
        var function = new Function(commands).SetLocation(
            new ResourceLocation(_datapack.Namespace, inst.Label)
        );
        _datapack.Functions.Add(function);
        if (inst.Label.Equals("global")) {
            // We need to tag minecraft:load for global chunk
            var load = _datapack.Tags.Find((tag) => (
                tag.Type == TagType.Function
                && tag.Location.Equals(new ResourceLocation("minecraft", "load"))
            ));
            if (load == null) return [];
            load.Entries.Add(function.Location.ToString());
        }
        return [];
    }

    public override List<string> VisitCommandInstruction(CommandInstruction inst) {
        return [inst.Command];
    }

    public override List<string> VisitJumpInstruction(JumpInstruction inst) {
        List<string> insts = [];
        var location = new ResourceLocation(
            _datapack.Namespace, inst.Target.Label
        );
        if (inst.IsFallThrough) {
            insts.Add($"scoreboard players set {Constants.FallThroughReturnHolder} {_datapack.Id} 0");
        }
        if (inst is ConditionalInstruction condInst) {
            // Added a condition check
            // The right operand is always ConstantOperand in JumpIfInstruction, you
            // can find it in IRBuilder
            if (condInst.Condition.Right is not ConstantOperand constant) return [];
            if (condInst.Condition.Left is ScoreboardOperand scoreboard) {
                var ifOrUnless = inst is JumpIfInstruction ? "if" : "unless";
                insts.Add($"execute {ifOrUnless} score {scoreboard.Code} {_datapack.Id} matches {constant.Value} store result score {Constants.FallThroughReturnHolder} {_datapack.Id} run function {location}");
            }
            // It can't be storage, because the left operand is always the result
            // of evaluating some logical expressions, whose result should be
            // a bool type, which is stored in Scoreboard.
        } else if (inst is JumpIfInstruction) {
            insts.Add($"execute store result score {Constants.FallThroughReturnHolder} {_datapack.Id} run function {location}");
        }
        return insts;
    }
    public override List<string> VisitCallInstruction(CallInstruction inst) {
        var location = new ResourceLocation(
            _datapack.Namespace, inst.Target.Label
        );
        return [$"execute store result score {Constants.FallThroughReturnHolder} {_datapack.Id} run function {location}"];
    }

    public override List<string> VisitLinkInstruction(LinkInstruction inst) {
        return [];
    }

    public override List<string> VisitMoveInstruction(MoveInstruction inst) {
        return (inst.Source, inst.Destination) switch {
            // Destination cannot be a ConstantOperand in a move operation.
            (_, ConstantOperand dest) =>
                [$"# ERROR: Destination cannot be a ConstantOperand: {dest.Value}"],

            // --- Destination is ScoreboardOperand ---
            (ConstantOperand source, ScoreboardOperand dest) =>
                // Move constant to scoreboard:
                // /scoreboard players set <target> <objective> <value>
                [$"scoreboard players set {dest.Code} {_datapack.Id} {source.GetValueInGame()}"],

            (ScoreboardOperand source, ScoreboardOperand dest) =>
                // Move scoreboard to scoreboard:
                // /scoreboard players operation <destTarget> <destObj> = <sourceTarget> <sourceObj>
                [$"scoreboard players operation {dest.Code} {_datapack.Id} = {source.Code} {_datapack.Id}"],

            (StorageOperand source, ScoreboardOperand dest) =>
                // Move storage to scoreboard:
                // /execute store result score <target> <objective> run data get storage <id> <path>
                [$"execute store result score {dest.Code} {_datapack.Id} run data get storage {_datapack.Id} {source.Code}"],

            // --- Destination is StorageOperand ---
            (ConstantOperand source, StorageOperand dest) =>
                // Move constant to storage:
                // /data modify storage <id> <path> set value <value>
                [$"data modify storage {_datapack.Id} {dest.Code} set value {source.GetValueInGame()}"],

            (ScoreboardOperand source, StorageOperand dest) =>
                // Move scoreboard to storage:
                // /execute store result storage <id> <path> float 1.0 run scoreboard players get <target> <objective>
                [$"execute store result storage {_datapack.Id} {dest.Code} float 1.0 run scoreboard players get {source.Code} {_datapack.Id}"],

            (StorageOperand source, StorageOperand dest) =>
                // Move storage to storage:
                // /data modify storage <destId> <destPath> set from storage <sourceId> <sourcePath>
                [$"data modify storage {_datapack.Id} {dest.Code} set from storage {_datapack.Id} {source.Code}"],

            _ => []
        };
    }

    public override List<string> VisitReturnIfInstruction(ReturnIfInstruction inst) {
        return (inst.Condition.Left, inst.Condition.Right) switch {
            (ConstantOperand left, ConstantOperand right) =>
                left.Value.Equals(right.Value) ? [$"return {GenOperand(inst.Value)}"] : [],

            // ----- Constant and Scoreboard -----
            (ScoreboardOperand left, ConstantOperand right) =>
                // /execute if score <destTarget> <destObj> matches <value>
                [$"execute if score {left.Code} {_datapack.Id} matches {GenOperand(right)} run return {GenOperand(inst.Value)}"],
            (ConstantOperand left, ScoreboardOperand right) =>
                // /execute if score <destTarget> <destObj> matches <value>
                [$"execute if score {right.Code} {_datapack.Id} matches {GenOperand(left)} run return {GenOperand(inst.Value)}"],
            (ScoreboardOperand left, ScoreboardOperand right) =>
                // Move scoreboard to scoreboard:
                // /execute if score <destTarget> <destObj> = <sourceTarget> <sourceObj>
                [$"execute if score {left.Code} {_datapack.Id} = {right.Code} {_datapack.Id} run return {GenOperand(inst.Value)}"],


            (ScoreboardOperand left, StorageOperand right) =>
                [],
            (StorageOperand left, ScoreboardOperand right) =>
                [],

            // --- Destination is StorageOperand ---
            (StorageOperand left, ConstantOperand right) =>
                [],
            (ConstantOperand left, StorageOperand right) =>
                [],
            (StorageOperand left, StorageOperand right) =>
                [],

            _ => []
        };
    }

    public override List<string> VisitReturnInstruction(ReturnInstruction inst) {
        return [$"return {GenOperand(inst.Value)}"];
    }

    public override List<string> VisitPushInstruction(PushInstruction inst) {
        if (inst.Operand is ScoreboardOperand scoreboard) {
            return [
                $"data modify storage minecraft:{_datapack.Id} {scoreboard.StackName} prepend value 0",
                $"execute store result storage minecraft:{_datapack.Id} {scoreboard.StackName}[0] int 1 run scoreboard players get {scoreboard.Code} {_datapack.Id}"
            ];
        }
        return [$"data modify storage minecraft:{_datapack.Id} {inst.Operand.StackName} prepend from storage minecraft:{_datapack.Id} {inst.Operand.Code}"];
    }

    public override List<string> VisitPopInstruction(PopInstruction inst) {
        if (inst.Operand is ScoreboardOperand scoreboard) {
            return [
                $"execute store result score {scoreboard.Code} {_datapack.Id} run data get storage minecraft:{_datapack.Id} {inst.Operand.StackName}[0] 1",
                $"data remove storage minecraft:{_datapack.Id} {scoreboard.StackName}[0]"
            ];
        }
        return [
            // Pop the value to the Operant.
            $"data modify storage minecraft:{_datapack.Id} {inst.Operand.Code} set from storage minecraft:{_datapack.Id} {inst.Operand.StackName}[0]",
            // Remove the element we've just poped.
            $"data remove storage minecraft:{_datapack.Id} {inst.Operand.StackName}[0]",
        ];
    }
}
