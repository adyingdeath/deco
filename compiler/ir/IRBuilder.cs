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
        node.Functions.ForEach((func) => {
            _inst.AddRange(func.Accept(this) ?? []);
        });
        return _inst;
    }

    public List<IRInstruction> VisitArgument(ArgumentNode node) {
        return [];
    }

    public List<IRInstruction> VisitAssignment(AssignmentNode node) {
        var insts = new List<IRInstruction>();
        // Evaluate the expression to get its operand
        var valueOperand = node.Expression.Accept(evaluator.Inst(insts));
        // Get the variable operand
        var variableOperand = node.Variable.Accept(evaluator);
        // Create move instruction
        insts.Add(new MoveInstruction(valueOperand, variableOperand));
        return insts;
    }

    public List<IRInstruction> VisitBinaryOp(BinaryOpNode node) {
        return [];
    }

    public List<IRInstruction> VisitBlock(BlockNode node) {
        var insts = new List<IRInstruction>();
        foreach (var statement in node.Statements) {
            insts.AddRange(statement.Accept(this) ?? []);
        }
        return insts;
    }

    public List<IRInstruction> VisitCommand(CommandNode node) {
        return [new CommandInstruction(node.Command)];
    }

    public List<IRInstruction> VisitExpressionStatement(ExpressionStatementNode node) {
        var insts = new List<IRInstruction>();
        // Evaluate the expression (instructions will be added to insts)
        // The result is discarded as this is a statement
        node.Expression.Accept(evaluator.Inst(insts));
        return insts;
    }

    public List<IRInstruction> VisitFakeBlock(FakeBlockNode node) {
        var insts = new List<IRInstruction>();
        foreach (var statement in node.Statements) {
            insts.AddRange(statement.Accept(this) ?? []);
        }
        return insts;
    }

    public List<IRInstruction> VisitFor(ForNode node) {
        return [];
    }

    public List<IRInstruction> VisitFunction(FunctionNode node) {
        var insts = new List<IRInstruction>();

        // Create function entry label
        var functionLabel = new LabelInstruction(node.Name.Name);
        insts.Add(functionLabel);

        // Process function body
        insts.AddRange(node.Body.Accept(this) ?? []);

        return insts;
    }

    public List<IRInstruction> VisitFunctionCall(FunctionCallNode node) {
        return [];
    }

    public List<IRInstruction> VisitIdentifier(IdentifierNode node) {
        return [];
    }

    public List<IRInstruction> VisitIf(IfNode node) {
        var insts = new List<IRInstruction>();
        // Evaluate condition
        var conditionOperand = node.Condition.Accept(evaluator.Inst(insts));

        // Create labels
        var elseLabel = new LabelInstruction("__else_" + Compiler.variableCodeGen.Next());
        var endLabel = new LabelInstruction("__endif_" + Compiler.variableCodeGen.Next());

        // Check if condition is true. Jump to else unless condition is true
        var oneOperand = new ConstantOperand("1");
        var condition = new Condition(ConditionType.Equal, conditionOperand, oneOperand);

        // Jump to else unless condition_operand == 1 (i.e. jump when condition is false)
        insts.Add(new JumpUnlessInstruction(condition, elseLabel));

        // Then block
        insts.AddRange(node.ThenBlock.Accept(this) ?? []);

        // If there's else block, jump over it
        if (node.ElseBlock != null) {
            insts.Add(new JumpInstruction(endLabel));
        }

        // Else label
        insts.Add(elseLabel);

        // Else block (if exists)
        if (node.ElseBlock != null) {
            insts.AddRange(node.ElseBlock.Accept(this) ?? []);
            insts.Add(endLabel);
        }

        return insts;
    }

    public List<IRInstruction> VisitLiteral(LiteralNode node) {
        return [];
    }

    public List<IRInstruction> VisitModifier(ModifierNode node) {
        return [];
    }

    public List<IRInstruction> VisitReturn(ReturnNode node) {
        var insts = new List<IRInstruction>();
        if (node.Expression != null) {
            // Evaluate the return value
            var returnValueOperand = node.Expression.Accept(evaluator.Inst(insts));
            insts.Add(new ReturnInstruction(returnValueOperand));
        } else {
            insts.Add(new ReturnInstruction());
        }
        return insts;
    }

    public List<IRInstruction> VisitUnaryOp(UnaryOpNode node) {
        return [];
    }

    public List<IRInstruction> VisitVariableDefinition(VariableDefinitionNode node) {
        return [];
    }

    public List<IRInstruction> VisitWhile(WhileNode node) {
        var insts = new List<IRInstruction>();

        // Create labels
        var loopStartLabel = new LabelInstruction("__while_start_" + Compiler.variableCodeGen.Next());
        var loopEndLabel = new LabelInstruction("__while_end_" + Compiler.variableCodeGen.Next());

        // Start label
        insts.Add(loopStartLabel);

        // Evaluate condition
        var conditionOperand = node.Condition.Accept(evaluator.Inst(insts));

        // Check if condition is true. Jump to end unless condition is true
        var oneOperand = new ConstantOperand("1");
        var condition = new Condition(ConditionType.Equal, conditionOperand, oneOperand);

        // Jump to end unless condition_operand == 1 (i.e. exit loop when condition is false)
        insts.Add(new JumpUnlessInstruction(condition, loopEndLabel));

        // Loop body
        insts.AddRange(node.Body.Accept(this) ?? []);

        // Jump back to start
        insts.Add(new JumpInstruction(loopStartLabel));

        // End label
        insts.Add(loopEndLabel);

        return insts;
    }
}
