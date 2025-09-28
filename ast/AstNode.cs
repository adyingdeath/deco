using Deco.Types;

namespace Deco.Ast;

public abstract class AstNode(int line = 0, int column = 0) {
    public int Line { get; set; } = line;
    public int Column { get; set; } = column;

    /// <summary>
    /// Optional reference to the symbol table scope associated with this node.
    /// Used for reference checking and other analyses during AST traversal.
    /// </summary>
    public Scope? Scope { get; set; }
    public AstNode? Parent { get; set; }

    public abstract T Accept<T>(IAstVisitor<T> visitor);
    public virtual AstNode CloneContext(AstNode node) {
        Scope = node.Scope;
        Parent = node.Parent;
        return this;
    }
    public virtual Scope? FindScope() {
        if (Scope != null) return Scope;
        return Parent?.FindScope();
    }
}

public interface IAstVisitor<T> {
    T VisitProgram(ProgramNode node);
    T VisitFunction(FunctionNode node);
    T VisitModifier(ModifierNode node);
    T VisitArgument(ArgumentNode node);
    T VisitExpressionStatement(ExpressionStatementNode node);
    T VisitCommand(CommandNode node);
    T VisitVariableDefinition(VariableDefinitionNode node);
    T VisitAssignment(AssignmentNode node);
    T VisitReturn(ReturnNode node);
    T VisitIf(IfNode node);
    T VisitWhile(WhileNode node);
    T VisitFor(ForNode node);
    T VisitBlock(BlockNode node);
    T VisitFakeBlock(FakeBlockNode node);
    T VisitBinaryOp(BinaryOpNode node);
    T VisitUnaryOp(UnaryOpNode node);
    T VisitLiteral(LiteralNode node);
    T VisitIdentifier(IdentifierNode node);
    T VisitFunctionCall(FunctionCallNode node);
}
