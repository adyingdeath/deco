namespace Deco.Compiler.IR;

/// <summary>
/// Visitor pattern for IR instructions. Visits labels first, then processes blocks.
/// Each visit method returns a list of IR instructions.
/// </summary>
public abstract class IRVisitor<T> {
    public abstract T VisitProgram(ProgramInstruction inst);

    public abstract T VisitLabelInstruction(LabelInstruction inst);

    public abstract T VisitMoveInstruction(MoveInstruction inst);

    public abstract T VisitBinaryInstruction(BinaryInstruction inst);

    public abstract T VisitJumpInstruction(JumpInstruction inst);

    public abstract T VisitLinkInstruction(LinkInstruction inst);

    public abstract T VisitReturnInstruction(ReturnInstruction inst);

    public abstract T VisitCommandInstruction(CommandInstruction inst);

    public abstract T VisitUnaryInstruction(UnaryInstruction inst);

    public abstract T VisitReturnIfInstruction(ReturnIfInstruction inst);
}
