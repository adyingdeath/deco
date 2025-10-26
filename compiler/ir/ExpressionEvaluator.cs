using Deco.Compiler.Types;
using Deco.Compiler.Ast;
using Deco.Compiler.Lib;

namespace Deco.Compiler.IR;

public class ExpressionEvaluator(CompilationContext context) : IAstVisitor<Operand> {
    private readonly CompilationContext _context = context;
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
        Operand left = node.Left.Accept(this);
        Operand right = node.Right.Accept(this);

        // We should have a temporary variable to store the intermediate result,
        // so we need to determine where the variable should be stored in. Here
        // we used to use complex logics to determine the temp variable's type,
        // but I move the logics to OperandUtils now.
        Operand temp = OperandUtils.CreateTemporaryForType(
            node.Type, _context.VariableCodeGen.Next()
        );

        switch (node.Operator) {
            case BinaryOperator.Add:
                Insts.Add(new AddInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.Subtract:
                Insts.Add(new SubtractInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.Multiply:
                Insts.Add(new MultiplyInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.Divide:
                Insts.Add(new DivideInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.Equal:
                Insts.Add(new EqualInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.NotEqual:
                Insts.Add(new NotEqualInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.LessThan:
                Insts.Add(new LessThanInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.LessThanOrEqual:
                Insts.Add(new LessThanOrEqualInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.GreaterThan:
                Insts.Add(new GreaterThanInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.GreaterThanOrEqual:
                Insts.Add(new GreaterThanOrEqualInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.LogicalAnd:
                Insts.Add(new LogicalAndInstruction(
                    temp,
                    left,
                    right
                ));
                break;
            case BinaryOperator.LogicalOr:
                Insts.Add(new LogicalOrInstruction(
                    temp,
                    left,
                    right
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
        // Get the function's symbol. We need symbols of its arguments and return
        if (scope.LookupSymbol(node.Name.Name) is not FunctionSymbol symbol) return null!;

        List<IRInstruction> popInsts = [];

        // Pass arguments using Move
        for (int i = 0; i < symbol.ParameterSymbol.Count; i++) {
            if (node.Arguments.Count < i) {
                throw new Exception($"function '{node.Name.Name}' should have {symbol.ParameterSymbol.Count} parameters, but got {node.Arguments.Count}.");
            }
            Operand argValue = node.Arguments[i].Accept(this);
            VariableOperand argVar = (VariableOperand)VariableOperand.Create(symbol.ParameterSymbol[i]);
            // Push the old value to stack, with its coresponding Pop. We will
            // reverse the pop list later.
            Insts.Add(new PushInstruction(argVar));
            popInsts.Add(new PopInstruction(argVar));
            // Move the calculated value to the parameter variable operand
            Insts.Add(new MoveInstruction(argValue, argVar));
        }

        if (symbol is LibraryFunctionSymbol libFunc) {
            // Handle specially the library function here.
            // Library functions don't need real Call, we just need to call its
            // `Run` method.
            var context = new Context();
            var paramArg = libFunc.ParameterSymbol.Select((sym) => {
                if (sym.Type.IsStorableInScoreboard) {
                    return new Argument(ArgumentType.SCOREBOARD, _context.Datapack.Id, sym.Code);
                } else {
                    return new Argument(ArgumentType.STORAGE, $"minecraft:{_context.Datapack.Id}", sym.Code);
                }
            }).ToList();
            var returnArg = libFunc.ReturnSymbol.Type.IsStorableInScoreboard
                ? new Argument(ArgumentType.SCOREBOARD, _context.Datapack.Id, libFunc.ReturnSymbol.Code)
                : new Argument(ArgumentType.STORAGE, $"minecraft:{_context.Datapack.Id}", libFunc.ReturnSymbol.Code);
            libFunc.Implementation.Run(context, paramArg, returnArg);
            // Now insert generated commands to the function call's position
            context.CommandList.ForEach((cmd) => {
                Insts.Add(new CommandInstruction(cmd));
            });
        } else {
            // Really call the function with a Call instruction
            Insts.Add(new CallInstruction(
                new LabelInstruction(symbol.Code)
            ));
        }

        // Pop old value from stack
        popInsts.Reverse();
        Insts.AddRange(popInsts);

        var returnSymbol = symbol.ReturnSymbol;
        if (returnSymbol == null || returnSymbol.Type.Equals(TypeUtils.VoidType)) {
            return null!;
        }
        return VariableOperand.Create(returnSymbol);
    }

    public Operand VisitIdentifier(IdentifierNode node) {
        var symbol = node.FindScope()?.LookupSymbol(node.Name)
            ?? throw new InvalidOperationException($"Symbol '{node.Name}' not found");
        return VariableOperand.Create(symbol);
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
        Operand temp = OperandUtils.ParseVariable(node.Operand.Type, _context.VariableCodeGen.Next());

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
