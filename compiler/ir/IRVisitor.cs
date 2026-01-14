namespace Deco.Compiler.IR;

/// <summary>
/// Visitor pattern for IR instructions.
/// </summary>
public abstract class IRVisitor<T> {
    // Top level structures
    public abstract T VisitProgram(IrProgram program);
    public abstract T VisitMoveInstruction(MoveInstruction inst);
    public abstract T VisitBinaryInstruction(BinaryInstruction inst);
    
    // --- Push and Pop ---
    public abstract T VisitPushInstruction(PushInstruction inst);
    public abstract T VisitPopInstruction(PopInstruction inst);

    // --- Binary Operation Types ---
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

    // --- Control Flow ---
    // Removed Jumps and Labels
    public abstract T VisitCallInstruction(CallInstruction inst);
    public abstract T VisitCallIfInstruction(CallIfInstruction inst);
    public abstract T VisitReturnInstruction(ReturnInstruction inst);
    public abstract T VisitReturnIfInstruction(ReturnIfInstruction inst);
    
    public abstract T VisitCommandInstruction(CommandInstruction inst);
    
    // --- Specific Unary Instruction Types ---
    public abstract T VisitUnaryInstruction(UnaryInstruction inst);
    public abstract T VisitNegateInstruction(NegateInstruction inst);
    public abstract T VisitLogicalNotInstruction(LogicalNotInstruction inst);
}