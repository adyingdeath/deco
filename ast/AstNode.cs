namespace Deco.Ast;

public abstract class AstNode(int line = 0, int column = 0) {
    public int Line { get; set; } = line;
    public int Column { get; set; } = column;

    public abstract T Accept<T>(IAstVisitor<T> visitor);
}

public interface IAstVisitor<T> {
    T VisitProgram(ProgramNode node);
    T VisitFunction(FunctionNode node);
    T VisitModifier(ModifierNode node);
    T VisitArgument(ArgumentNode node);
    T VisitCommand(CommandNode node);
    T VisitVariableDefinition(VariableDefinitionNode node);
    T VisitAssignment(AssignmentNode node);
    T VisitReturn(ReturnNode node);
    T VisitIf(IfNode node);
    T VisitWhile(WhileNode node);
    T VisitBlock(BlockNode node);
    T VisitBinaryOp(BinaryOpNode node);
    T VisitUnaryOp(UnaryOpNode node);
    T VisitLiteral(LiteralNode node);
    T VisitIdentifier(IdentifierNode node);
    T VisitFunctionCall(FunctionCallNode node);
}
