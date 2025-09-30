namespace Deco.Compiler.IR.Passes;

public class LinkMergePass {
    public static List<IRInstruction> Visit(List<IRInstruction> irs) {
        Dictionary<string, int> labels = [];
        List<IRInstruction> instructions = [];
        // Collect all the labels and its index
        for (int i = 0; i < irs.Count; i++) {
            if (irs[i] is LabelInstruction label) {
                labels[label.Label] = i;
            }
        }
        for (int i = 0; i < irs.Count;) {
            if (irs[i] is LabelInstruction label && label.IsAnchor) {
                // Just skip all the instructions in it for Anchor Label.
                while (++i < irs.Count) {
                    if (irs[i] is LabelInstruction) break;
                }
                continue;
            } else if (irs[i] is LinkInstruction link) {
                if (!labels.TryGetValue(link.Target.Label, out int pos)) {
                    i++;
                    continue;
                }
                int end = pos;
                while (++end < irs.Count) {
                    if (irs[end] is LabelInstruction) break;
                    instructions.Add(irs[end]);
                }
                i++;
                continue;
            }
            instructions.Add(irs[i]);
            i++;
        }
        return instructions;
    }
}
