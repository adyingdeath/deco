using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.IR;

public class IRBuilder : IAstVisitor<List<IRInstruction>> {
    public ExpressionEvaluator evaluator = new();
    public List<IRInstruction> Instructions = [];

    public List<IRInstruction> VisitProgram(ProgramNode node) {
        List<IRInstruction> _inst = [];
        node.VariableDefinitions.ForEach((varDef) => {
            var symbol = node.Scope?.LookupSymbol(varDef.Name.Name);
            if (symbol == null) return;
            var scoreboard = TypeUtils.IsScoreboard(symbol.Type);
            Operand variable = scoreboard
                ? new ScoreboardOperand(symbol.Code)
                : new StorageOperand(symbol.Code);
            // Create a MOVE instruction to set the initial value of the variable
            if (varDef.InitialValue != null) {
                // If there is an initial value, we should evaluate the initial 
                // value first
                var operand = varDef.InitialValue.Accept(evaluator.Inst(_inst));
                _inst.Add(new MoveInstruction(
                    operand,
                    variable
                ));
            } else {
                var initial = TypeUtils.GetInitialValue(symbol.Type);
                _inst.Add(new MoveInstruction(
                    new ConstantOperand(initial),
                    variable
                ));
            }
        });
        return _inst;
    }

    public List<IRInstruction> VisitArgument(ArgumentNode node) {
        return null!;
    }

    public List<IRInstruction> VisitAssignment(AssignmentNode node) {
        return null!;
    }

    public List<IRInstruction> VisitBinaryOp(BinaryOpNode node) {
        return null!;
    }

    public List<IRInstruction> VisitBlock(BlockNode node) {
        return null!;
    }

    public List<IRInstruction> VisitCommand(CommandNode node) {
        return [new CommandInstruction(node.Command)];
    }

    public List<IRInstruction> VisitExpressionStatement(ExpressionStatementNode node) {
        return null!;
    }

    public List<IRInstruction> VisitFakeBlock(FakeBlockNode node) {
        return null!;
    }

    public List<IRInstruction> VisitFor(ForNode node) {
        return null!;
    }

    public List<IRInstruction> VisitFunction(FunctionNode node) {
        return null!;
    }

    public List<IRInstruction> VisitFunctionCall(FunctionCallNode node) {
        return null!;
    }

    public List<IRInstruction> VisitIdentifier(IdentifierNode node) {
        return null!;
    }

    public List<IRInstruction> VisitIf(IfNode node) {
        return null!;
    }

    public List<IRInstruction> VisitLiteral(LiteralNode node) {
        return null!;
    }

    public List<IRInstruction> VisitModifier(ModifierNode node) {
        return null!;
    }

    public List<IRInstruction> VisitReturn(ReturnNode node) {
        return null!;
    }

    public List<IRInstruction> VisitUnaryOp(UnaryOpNode node) {
        return null!;
    }

    public List<IRInstruction> VisitVariableDefinition(VariableDefinitionNode node) {
        return null!;
    }

    public List<IRInstruction> VisitWhile(WhileNode node) {
        return null!;
    }
}