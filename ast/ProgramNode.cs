using System.Collections.Generic;

namespace Deco.Ast;

public class ProgramNode(List<FunctionNode> functions, int line = 0, int column = 0) : AstNode(line, column) {
    public List<FunctionNode> Functions { get; } = functions ?? [];

    public override T Accept<T>(IAstVisitor<T> visitor) {
        return visitor.VisitProgram(this);
    }
}
