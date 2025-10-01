using Deco.Compiler.IR;

namespace Deco.Compiler.Pack;

/// <summary>
/// Builds a Datapack from a list of IR instructions.
/// </summary>
public class DatapackBuilder(Datapack datapack) : IRVisitor<List<string>> {
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

    public override List<string> VisitBinaryInstruction(BinaryInstruction inst) {
        return [];
    }

    public override List<string> VisitCommandInstruction(CommandInstruction inst) {
        return [inst.Command];
    }

    public override List<string> VisitJumpInstruction(JumpInstruction inst) {
        var location = new ResourceLocation(
            _datapack.Namespace, inst.Target.Label
        );
        return [$"function {location}"];
    }

    public override List<string> VisitLinkInstruction(LinkInstruction inst) {
        return [];
    }

    public override List<string> VisitMoveInstruction(MoveInstruction inst)
    {
        var commands = new List<string>();

        switch (inst.Source, inst.Destination)
        {
            // Destination cannot be a ConstantOperand in a move operation.
            case (_, ConstantOperand dest):
                commands.Add($"# ERROR: Destination cannot be a ConstantOperand: {dest.Value}");
                break;

            // --- Destination is ScoreboardOperand ---
            case (ConstantOperand source, ScoreboardOperand dest):
                // Move constant to scoreboard:
                // /scoreboard players set <target> <objective> <value>
                commands.Add($"scoreboard players set {dest.Code} {_datapack.Id} {source.Value}");
                break;

            case (ScoreboardOperand source, ScoreboardOperand dest):
                // Move scoreboard to scoreboard:
                // /scoreboard players operation <destTarget> <destObj> = <sourceTarget> <sourceObj>
                commands.Add($"scoreboard players operation {dest.Code} {_datapack.Id} = {source.Code} {_datapack.Id}");
                break;

            case (StorageOperand source, ScoreboardOperand dest):
                // Move storage to scoreboard:
                // /execute store result score <target> <objective> run data get storage <id> <path>
                commands.Add($"execute store result score {dest.Code} {_datapack.Id} run data get storage {_datapack.Id} {source.Code}");
                break;

            // --- Destination is StorageOperand ---
            case (ConstantOperand source, StorageOperand dest):
                // Move constant to storage:
                // /data modify storage <id> <path> set value <value>
                commands.Add($"data modify storage {_datapack.Id} {dest.Code} set value {source.Value}");
                break;

            case (ScoreboardOperand source, StorageOperand dest):
                // Move scoreboard to storage:
                // /execute store result storage <id> <path> float 1.0 run scoreboard players get <target> <objective>
                commands.Add($"execute store result storage {_datapack.Id} {dest.Code} float 1.0 run scoreboard players get {source.Code} {_datapack.Id}");
                break;

            case (StorageOperand source, StorageOperand dest):
                // Move storage to storage:
                // /data modify storage <destId> <destPath> set from storage <sourceId> <sourcePath>
                commands.Add($"data modify storage {_datapack.Id} {dest.Code} set from storage {_datapack.Id} {source.Code}");
                break;

            // Fallback for unhandled combinations (should not be reached if all 3x3 cases are covered, except invalid dest constant)
            default:
                commands.Add($"# WARNING: Unhandled MoveInstruction combination: Source={inst.Source.GetType().Name}, Destination={inst.Destination.GetType().Name}");
                break;
        }

        return commands;
    }

    public override List<string> VisitReturnIfInstruction(ReturnIfInstruction inst) {
        return [];
    }

    public override List<string> VisitReturnInstruction(ReturnInstruction inst) {
        return [];
    }

    public override List<string> VisitUnaryInstruction(UnaryInstruction inst) {
        return [];
    }
}
