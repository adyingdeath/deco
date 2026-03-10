using Deco.Compiler.Types;
using Deco.Compiler.Ast;
using Deco.Compiler.Lib.Api;

namespace Deco.Compiler.IR;

public class ExpressionEvaluator(CompilationContext context) : IAstVisitor<Operand> {
    private readonly CompilationContext _context = context;
    private List<IRInstruction> Insts = [];
    
    /// <summary>
    /// Sets the target instruction list where generated code will be appended.
    /// </summary>
    public ExpressionEvaluator Inst(List<IRInstruction> irs) {
        Insts = irs;
        return this;
    }

    public Operand VisitProgram(ProgramNode node) => throw new NotImplementedException();
    public Operand VisitArgument(ArgumentNode node) => throw new NotImplementedException();
    public Operand VisitAssignment(AssignmentNode node) => throw new NotImplementedException();
    public Operand VisitBlock(BlockNode node) => throw new NotImplementedException();
    public Operand VisitCommand(CommandNode node) => throw new NotImplementedException();
    public Operand VisitExpressionStatement(ExpressionStatementNode node) => throw new NotImplementedException();
    public Operand VisitFor(ForNode node) => throw new NotImplementedException();
    public Operand VisitFunction(FunctionNode node) => throw new NotImplementedException();
    public Operand VisitIf(IfNode node) => throw new NotImplementedException();
    public Operand VisitModifier(ModifierNode node) => throw new NotImplementedException();
    public Operand VisitReturn(ReturnNode node) => throw new NotImplementedException();
    public Operand VisitVariableDefinition(VariableDefinitionNode node) => throw new NotImplementedException();
    public Operand VisitWhile(WhileNode node) => throw new NotImplementedException();

    public Operand VisitBinaryOp(BinaryOpNode node) {
        Operand left = node.Left.Accept(this);
        Operand right = node.Right.Accept(this);

        Operand temp = OperandUtils.CreateTemporaryForType(
            node.Type, _context.VariableCodeGen.Next()
        );

        IRInstruction inst = node.Operator switch {
            BinaryOperator.Add => new AddInstruction(temp, left, right),
            BinaryOperator.Subtract => new SubtractInstruction(temp, left, right),
            BinaryOperator.Multiply => new MultiplyInstruction(temp, left, right),
            BinaryOperator.Divide => new DivideInstruction(temp, left, right),
            BinaryOperator.Equal => new EqualInstruction(temp, left, right),
            BinaryOperator.NotEqual => new NotEqualInstruction(temp, left, right),
            BinaryOperator.LessThan => new LessThanInstruction(temp, left, right),
            BinaryOperator.LessThanOrEqual => new LessThanOrEqualInstruction(temp, left, right),
            BinaryOperator.GreaterThan => new GreaterThanInstruction(temp, left, right),
            BinaryOperator.GreaterThanOrEqual => new GreaterThanOrEqualInstruction(temp, left, right),
            BinaryOperator.LogicalAnd => new LogicalAndInstruction(temp, left, right),
            BinaryOperator.LogicalOr => new LogicalOrInstruction(temp, left, right),
            _ => throw new NotImplementedException($"Binary operator {node.Operator} not implemented")
        };
        
        Insts.Add(inst);
        return temp;
    }

    public Operand VisitUnaryOp(UnaryOpNode node) {
        Operand temp = OperandUtils.ParseVariable(node.Operand.Type, _context.VariableCodeGen.Next());
        IRInstruction inst = node.Operator switch {
            UnaryOperator.Negate => new NegateInstruction(temp, node.Operand.Accept(this)),
            UnaryOperator.LogicalNot => new LogicalNotInstruction(temp, node.Operand.Accept(this)),
            _ => throw new NotImplementedException($"Unary operator {node.Operator} not implemented")
        };
        Insts.Add(inst);
        return temp;
    }

    public Operand VisitLiteral(LiteralNode node) {
        return new ConstantOperand(node.Value);
    }

    public Operand VisitIdentifier(IdentifierNode node) {
        var symbol = node.FindScope()?.LookupSymbol(node.Name)
            ?? throw new InvalidOperationException($"Symbol '{node.Name}' not found");
        return VariableOperand.Create(symbol);
    }

    public Operand VisitFunctionCall(FunctionCallNode node) {
        var scope = node.FindScope();
        if (scope == null) return null!;
        
        var symbol = scope.LookupSymbol(node.Name.Name);
        if (symbol is not FunctionSymbol funcSymbol) return null!;

        // --- NEW: Handle Library Functions (Inline Generation) ---
        if (symbol is LibraryFunctionSymbol libFunc) {
            // 1. Evaluate arguments first
            var evaluatedArgs = new List<Operand>();
            foreach (var argNode in node.Arguments) {
                evaluatedArgs.Add(argNode.Accept(this));
            }

            // 2. Prepare context
            var libCtx = new LibraryContext(_context, new IRBuilder(_context), Insts);

            // 3. Prepare result operand (if not void)
            Operand? resultOperand = null;
            if (!libFunc.ReturnSymbol.Type.Equals(TypeUtils.VoidType)) {
                resultOperand = OperandUtils.CreateTemporaryForType(
                    libFunc.ReturnSymbol.Type, 
                    _context.VariableCodeGen.Next()
                );
            }

            // 4. Generate IR via the delegate
            libFunc.Generator(libCtx, evaluatedArgs, resultOperand);

            return resultOperand ?? null!;
        }

        // --- Standard Function Call (Push/Pop/Call) ---
        List<IRInstruction> popInsts = [];

        // Pass arguments using Move
        for (int i = 0; i < funcSymbol.ParameterSymbol.Count; i++) {
            if (node.Arguments.Count <= i) {
                 throw new Exception($"function '{node.Name.Name}' missing arguments.");
            }
            
            Operand argValue = node.Arguments[i].Accept(this);
            VariableOperand argVar = (VariableOperand)VariableOperand.Create(funcSymbol.ParameterSymbol[i]);
            
            // Push old value, move new value
            Insts.Add(new PushInstruction(argVar));
            popInsts.Add(new PopInstruction(argVar));
            Insts.Add(new MoveInstruction(argValue, argVar));
        }

        Insts.Add(new CallInstruction(funcSymbol.Code));

        // Restore old values
        popInsts.Reverse();
        Insts.AddRange(popInsts);

        // Handle Return Value
        var returnSymbol = funcSymbol.ReturnSymbol;
        if (returnSymbol == null || returnSymbol.Type.Equals(TypeUtils.VoidType)) {
            return null!;
        }
        return VariableOperand.Create(returnSymbol);
    }
}
