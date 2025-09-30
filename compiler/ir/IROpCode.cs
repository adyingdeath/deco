namespace Deco.Compiler.IR;

/// <summary>
/// Enumeration of all IR opcodes. Each opcode represents a specific instruction type.
/// </summary>
public enum IROpCode {
    // Control flow
    Nop,
    Label,
    Jump,
    JumpIf,
    JumpUnless,
    Return,
    ReturnIf,

    // Variables and assignment
    Move,

    // Arithmetic operations
    Add,
    Subtract,
    Multiply,
    Divide,

    // Comparison operations
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,

    // Logical operations
    LogicalAnd,
    LogicalOr,
    LogicalNot,

    // Unary operations
    Negate,

    // Function calls
    CallFunction,

    // Commands
    Command,

    // Stack operations for expression evaluation
    Push,
    Pop,
}
