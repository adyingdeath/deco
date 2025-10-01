namespace Deco.Compiler.IR.Passes;

public class LinkMergePass {
    public static ProgramInstruction Visit(ProgramInstruction program) {
        // Process each label's instructions
        foreach (LabelInstruction label in program.Labels) {
            ProcessInstructions(label.Instructions);
        }

        // Remove all anchor labels after inlining
        program.Labels.RemoveAll(label => label.IsAnchor);

        return program;
    }

    private static void ProcessInstructions(List<IRInstruction> instrs) {
        for (int i = 0; i < instrs.Count; ) {
            if (instrs[i] is LinkInstruction link) {
                // Replace the link with the target's instructions
                instrs.RemoveAt(i);
                // Skip anchor labels
                if (!link.Target.IsAnchor) {
                    instrs.InsertRange(i, link.Target.Instructions);
                }
            } else {
                i++;
            }
        }
    }
}
