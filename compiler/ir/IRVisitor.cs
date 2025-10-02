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
    // --- Specific Binary Instruction Types ---
    public abstract T VisitAddInstruction(AddInstruction inst);
    public abstract T VisitSubtractInstruction(SubtractInstruction inst);
    public abstract T VisitMultiplyInstruction(MultiplyInstruction inst);
    public abstract T VisitDivideInstruction(DivideInstruction inst);
    public abstract T VisitEqualInstruction(EqualInstruction inst);
    public abstract T VisitNotEqualInstruction(NotEqualInstruction inst);
    public abstract T VisitLessThanInstruction(LessThanInstruction inst);
    public abstract T VisitLessThanOrEqualInstruction(LessThanOrEqualInstruction inst);
    public abstract T VisitGreaterThanInstruction(GreaterThanInstruction inst);
    public abstract T VisitGreaterThanOrEqualInstruction(GreaterThanOrEqualInstruction inst);
    public abstract T VisitLogicalAndInstruction(LogicalAndInstruction inst);
    public abstract T VisitLogicalOrInstruction(LogicalOrInstruction inst);
    public abstract T VisitJumpInstruction(JumpInstruction inst);
    public abstract T VisitCallInstruction(CallInstruction inst);
    public abstract T VisitLinkInstruction(LinkInstruction inst);
    public abstract T VisitReturnInstruction(ReturnInstruction inst);
    public abstract T VisitCommandInstruction(CommandInstruction inst);
    public abstract T VisitUnaryInstruction(UnaryInstruction inst);
    // --- Specific Unary Instruction Types ---
    public abstract T VisitNegateInstruction(NegateInstruction inst);
    public abstract T VisitLogicalNotInstruction(LogicalNotInstruction inst);
    public abstract T VisitReturnIfInstruction(ReturnIfInstruction inst);
}
