namespace Deco.Compiler.IR.Passes;

public class NestInstructionPass {
    public static ProgramInstruction Visit(List<IRInstruction> instrs) {
        ProgramInstruction program = new();
        LabelInstruction? currentLabel = null;

        foreach (IRInstruction instr in instrs) {
            if (instr is LabelInstruction label) {
                currentLabel = label;
                program.Labels.Add(label);
            } else {
                currentLabel?.Instructions.Add(instr);
            }
        }

        return program;
    }
}
