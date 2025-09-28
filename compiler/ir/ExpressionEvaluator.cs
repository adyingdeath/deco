using Deco.Ast;
using Deco.Types;

namespace Deco.Compiler.IR;

public class ExpressionEvaluator : IAstVisitor<Operand> {
    private List<IRInstruction> Insts = [];
    public ExpressionEvaluator Inst(List<IRInstruction> irs) {
        Insts = irs;
        return this;
    }

    public Operand VisitProgram(ProgramNode node) {
        throw new NotImplementedException();
    }
    
    public Operand VisitArgument(ArgumentNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitAssignment(AssignmentNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitBinaryOp(BinaryOpNode node) {
        // We should have a temporary variable to store the intermediate
        // result, so we need to determine where the variable should be
        // stored in.
        var bothScoreboard = TypeUtils.IsScoreboard(node.Left.Type)
            && TypeUtils.IsScoreboard(node.Right.Type);
        Operand temp = bothScoreboard
            ? new ScoreboardOperand(Compiler.variableCodeGen.Next())
            : new StorageOperand(Compiler.variableCodeGen.Next());

        switch (node.Operator) {
            case BinaryOperator.Add:
                Insts.Add(new AddInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.Subtract:
                Insts.Add(new SubtractInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.Multiply:
                Insts.Add(new MultiplyInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.Divide:
                Insts.Add(new DivideInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.Equal:
                Insts.Add(new EqualInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.NotEqual:
                Insts.Add(new NotEqualInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.LessThan:
                Insts.Add(new LessThanInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.LessThanOrEqual:
                Insts.Add(new LessThanOrEqualInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.GreaterThan:
                Insts.Add(new GreaterThanInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.GreaterThanOrEqual:
                Insts.Add(new GreaterThanOrEqualInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.LogicalAnd:
                Insts.Add(new LogicalAndInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            case BinaryOperator.LogicalOr:
                Insts.Add(new LogicalOrInstruction(
                    temp,
                    node.Left.Accept(this),
                    node.Right.Accept(this)
                ));
                break;
            default:
                throw new NotImplementedException($"Binary operator {node.Operator} not implemented");
        }
        return temp;
    }

    public Operand VisitBlock(BlockNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitCommand(CommandNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitExpressionStatement(ExpressionStatementNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitFakeBlock(FakeBlockNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitFor(ForNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitFunction(FunctionNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitFunctionCall(FunctionCallNode node) {
        var scope = node.FindScope();
        if (scope == null) return null!;
        var symbol = scope.LookupSymbol(node.Name.Name);
        if (symbol == null) return null!;
        FunctionType type = (FunctionType)symbol.Type;
        for (int i = 0; i < type.ParameterTypes.Count; i++) {
            if (node.Arguments.Count < i) {
                throw new Exception($"function '{node.Name.Name}' should have {type.ParameterTypes.Count} parameters, but got {node.Arguments.Count}.");
            }
            Operand argValue = node.Arguments[i].Accept(this);
            var argSymbol = scope.LookupSymbol(type.ParameterTypes[i].Name);
            if (argSymbol == null) continue;
            Insts.Add(new MoveInstruction(argValue, VariableOperand.Create(argSymbol)));
        }

        // Find the special symbol for function return value. The symbol's name
        // is "{functionName}#return". The creation of return symbol can be
        // found in ScopedSymbolTableBuilder.cs
        var returnSymbol = scope.LookupSymbol($"{node.Name.Name}#return");
        if (returnSymbol == null || returnSymbol.Type.Equals(TypeUtils.VoidType)) {
            return null!;
        }
        return VariableOperand.Create(returnSymbol);
    }

    public Operand VisitIdentifier(IdentifierNode node) {
        var symbol = node.FindScope()?.LookupSymbol(node.Name)
            ?? throw new InvalidOperationException($"Symbol '{node.Name}' not found");
        var scoreboard = TypeUtils.IsScoreboard(symbol.Type);
        return scoreboard
            ? new ScoreboardOperand(symbol.Code)
            : new StorageOperand(symbol.Code);
    }

    public Operand VisitIf(IfNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitLiteral(LiteralNode node) {
        return new ConstantOperand(node.Value);
    }

    public Operand VisitModifier(ModifierNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitReturn(ReturnNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitUnaryOp(UnaryOpNode node) {
        // Create temporary variable based on operand type
        var scoreboard = TypeUtils.IsScoreboard(node.Operand.Type);
        Operand temp = scoreboard
            ? new ScoreboardOperand(Compiler.variableCodeGen.Next())
            : new StorageOperand(Compiler.variableCodeGen.Next());

        switch (node.Operator) {
            case UnaryOperator.Negate:
                Insts.Add(new NegateInstruction(
                    temp,
                    node.Operand.Accept(this)
                ));
                break;
            case UnaryOperator.LogicalNot:
                Insts.Add(new LogicalNotInstruction(
                    temp,
                    node.Operand.Accept(this)
                ));
                break;
            default:
                throw new NotImplementedException($"Unary operator {node.Operator} not implemented");
        }
        return temp;
    }

    public Operand VisitVariableDefinition(VariableDefinitionNode node) {
        throw new NotImplementedException();
    }

    public Operand VisitWhile(WhileNode node) {
        throw new NotImplementedException();
    }
}
