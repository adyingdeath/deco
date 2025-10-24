using Deco.Compiler.Types;
using Deco.Compiler.Ast;
using Deco.Compiler.Pack;

namespace Deco.Compiler.IR;

public class IRBuilder(Datapack datapack) : IAstVisitor<List<IRInstruction>> {
    private Datapack _datapack = datapack;
    public ExpressionEvaluator evaluator = new(datapack);
    public List<IRInstruction> Instructions = [];

    private void BuildVariableDefinition(VariableDefinitionNode varDef, List<IRInstruction> insts) {
        // Find the variable's symbol definition. The symbol should exist,
        // because we've done checks in semantic analysis phase.
        Symbol symbol = varDef.FindScope()?.LookupSymbol(varDef.Name.Name)!;

        // Determine if the variable should be stored in scoreboard or storage.
        Operand variable = VariableOperand.Create(symbol);

        // Create a MOVE instruction to set the initial value of the variable
        if (varDef.InitialValue != null) {
            // If there is an initial value, we should evaluate the initial 
            // value first
            var initialValueOperand = varDef.InitialValue.Accept(evaluator.Inst(insts));
            insts.Add(new MoveInstruction(
                initialValueOperand,
                variable
            ));
        } else {
            // If no initial value, use default value
            var initial = symbol.Type.GetDefaultValueAsString();
            insts.Add(new MoveInstruction(
                new ConstantOperand(initial),
                variable
            ));
        }
    }

    public List<IRInstruction> VisitProgram(ProgramNode node) {
        List<IRInstruction> _inst = [];
        _inst.Add(new LabelInstruction("global"));
        // Initialize the main scoreboard and the main storage
        _inst.Add(new CommandInstruction($"scoreboard objectives remove {_datapack.Id}"));
        _inst.Add(new CommandInstruction($"scoreboard objectives add {_datapack.Id} dummy"));
        _inst.Add(new CommandInstruction($"data modify storage minecraft:{_datapack.Id} String set value []"));
        _inst.Add(new CommandInstruction($"data modify storage minecraft:{_datapack.Id} Double set value []"));
        _inst.Add(new CommandInstruction($"data modify storage minecraft:{_datapack.Id} Float set value []"));
        _inst.Add(new CommandInstruction($"data modify storage minecraft:{_datapack.Id} Int set value []"));
        //_inst.Add(new CommandInstruction($"data remove storage minecraft:{_datapack.Id}"));
        node.VariableDefinitions.ForEach((varDef) => {
            BuildVariableDefinition(varDef, _inst);
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
        var functionSymbol = node.FindScope()?.LookupSymbol(node.Name.Name);
        var functionLabel = new LabelInstruction(functionSymbol?.Code ?? node.Name.Name);
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
        var thenLabel = new LabelInstruction("__if_then_" + Compiler.functionCodeGen.Next(8));
        var elseLabel = new LabelInstruction("__if_else_" + Compiler.functionCodeGen.Next(8));

        // Link to the __if_end_
        var endLabel = new LabelInstruction("__link_if_end_" + Compiler.functionCodeGen.Next(8), true);
        var linkEnd = new LinkInstruction(endLabel);

        // Check if condition is true. Jump to else unless condition is true
        var condition = new Condition(
            ConditionType.Equal,
            conditionOperand,
            new ConstantOperand("1")
        );

        // Jump to then if condition_operand == 1
        // or else unless condition_operand == 1
        insts.Add(new JumpIfInstruction(condition, thenLabel).FallThrough());
        if(node.ElseBlock != null)
            insts.Add(new JumpUnlessInstruction(condition, elseLabel).FallThrough());

        // Do fall through here.
        insts.Add(new FallThroughInstruction());

        // Directly link to the end of if here
        insts.Add(linkEnd);

        // Then block
        insts.Add(thenLabel);
        insts.AddRange(node.ThenBlock.Accept(this) ?? []);

        // Else block (if exists)
        if (node.ElseBlock != null) {
            insts.Add(elseLabel);
            insts.AddRange(node.ElseBlock.Accept(this) ?? []);
        }

        insts.Add(endLabel);

        return insts;
    }

    public List<IRInstruction> VisitLiteral(LiteralNode node) {
        return [];
    }

    public List<IRInstruction> VisitModifier(ModifierNode node) {
        return [];
    }

    public List<IRInstruction> VisitReturn(ReturnNode node) {
        // Find its coresponding FunctionNode. We need its symbol
        var scope = node.FindScope();
        var functionNode = node.FindParent<FunctionNode>();
        if (functionNode == null || scope == null) return [];
        // Get the function's symbol. We need symbols of its arguments and return
        if (scope.LookupSymbol(functionNode.Name.Name) is not FunctionSymbol symbol) return [];

        var insts = new List<IRInstruction>();
        if (node.Expression != null) {
            // Evaluate the return value
            var returnValueOperand = node.Expression.Accept(evaluator.Inst(insts));
            // Store the return value to the function's return symbol
            insts.Add(new MoveInstruction(
                returnValueOperand, VariableOperand.Create(symbol.ReturnSymbol)
            ));
        }
        insts.Add(new ReturnInstruction(new ConstantOperand("1")));
        return insts;
    }

    public List<IRInstruction> VisitUnaryOp(UnaryOpNode node) {
        return [];
    }

    public List<IRInstruction> VisitVariableDefinition(VariableDefinitionNode node) {
        var insts = new List<IRInstruction>();
        BuildVariableDefinition(node, insts);
        return insts;
    }

    public List<IRInstruction> VisitWhile(WhileNode node) {
        var insts = new List<IRInstruction>();

        // Create labels
        var loopStartLabel = new LabelInstruction("__while_start_" + Compiler.functionCodeGen.Next(8));
        var loopEndLabel = new LabelInstruction("__while_end_" + Compiler.functionCodeGen.Next(8));

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
