using Deco.Compiler.Types;
using Deco.Compiler.Ast;

namespace Deco.Compiler.IR;

/// <summary>
/// Converts the AST into an IR Program consisting of discrete Functions.
/// </summary>
public class IRBuilder(CompilationContext context) : IAstVisitor<List<IRInstruction>> {
    private readonly CompilationContext _context = context;
    public ExpressionEvaluator evaluator = new(context);

    // This list holds all the functions generated during the build process.
    // The "Main" program flow will be just one of these functions (usually 'global').
    private readonly List<IrFunction> _generatedFunctions = [];

    /// <summary>
    /// Helper to register a new function node.
    /// </summary>
    private void RegisterFunction(string name, List<IRInstruction> instructions) {
        _generatedFunctions.Add(new IrFunction(name, instructions));
    }

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
            insts.Add(new MoveInstruction(initialValueOperand, variable));
        } else {
            // If no initial value, use default value
            var initial = symbol.Type.GetDefaultValueAsString();
            insts.Add(new MoveInstruction(new ConstantOperand(initial), variable));
        }
    }

    // Since we now return the whole IrProgram structure, we change the return type logic slightly
    // in the calling code, but here we keep the interface returning List<IRInstruction>
    // which represents the instructions of the *current* block being visited.

    public IrProgram Build(ProgramNode node) {
        // This is the entry point
        List<IRInstruction> globalInsts = VisitProgram(node);

        // Ensure the global function is added
        // The VisitProgram usually generates the body of "global"
        // We can prepend the initialization commands here if they aren't already.

        // --- Initialization commands here --- //

        // Note: VisitProgram creates the "global" body instructions. 
        // We create the "global" function wrapper here.
        var globalFunc = new IrFunction("global", globalInsts);

        // Insert global at the beginning if preferred, or just add it.
        _generatedFunctions.Insert(0, globalFunc);

        return new IrProgram {
            DataPackId = _context.Datapack.Id,
            Functions = [.. _generatedFunctions]
        };
    }

    public List<IRInstruction> VisitProgram(ProgramNode node) {
        List<IRInstruction> insts = [];

        // Initialize the main scoreboard and the main storage
        insts.Add(new CommandInstruction($"scoreboard objectives remove {_context.Datapack.Id}"));
        insts.Add(new CommandInstruction($"scoreboard objectives add {_context.Datapack.Id} dummy"));
        insts.Add(new CommandInstruction($"data modify storage minecraft:{_context.Datapack.Id} String set value []"));
        insts.Add(new CommandInstruction($"data modify storage minecraft:{_context.Datapack.Id} Double set value []"));
        insts.Add(new CommandInstruction($"data modify storage minecraft:{_context.Datapack.Id} Float set value []"));
        insts.Add(new CommandInstruction($"data modify storage minecraft:{_context.Datapack.Id} Int set value []"));
        //insts.Add(new CommandInstruction($"data remove storage minecraft:{_datapack.Id}"));
        node.VariableDefinitions.ForEach((varDef) => {
            BuildVariableDefinition(varDef, insts);
        });

        // User defined functions are separate top-level functions.
        // We visit them, but they register themselves into _generatedFunctions.
        // They do NOT return instructions to the 'global' flow.
        node.Functions.ForEach((func) => {
            func.Accept(this);
        });

        return insts;
    }

    public List<IRInstruction> VisitFunction(FunctionNode node) {
        var functionSymbol = node.FindScope()?.LookupSymbol(node.Name.Name);
        string funcName = functionSymbol?.Code ?? node.Name.Name;

        // Generate instructions for the function body
        List<IRInstruction> bodyInsts = node.Body.Accept(this) ?? [];

        // Register this as a standalone function
        RegisterFunction(funcName, bodyInsts);

        /* Return empty because this function definition doesn't add instructions 
        to the parent flow. */
        return [];
    }

    public List<IRInstruction> VisitIf(IfNode node) {
        var insts = new List<IRInstruction>();

        // Evaluate Condition
        var conditionOperand = node.Condition.Accept(evaluator.Inst(insts));
        var condition = new Condition(ConditionType.Equal, conditionOperand, new ConstantOperand("1"));

        // Extract "Then" Block to a new Function
        string thenFuncName = $"__if_then_{_context.FunctionCodeGen.Next(8)}";
        List<IRInstruction> thenInsts = node.ThenBlock.Accept(this) ?? [];
        RegisterFunction(thenFuncName, thenInsts);

        // Add Call to Then Function
        // if (cond == 1) call thenFunc
        insts.Add(new CallIfInstruction(condition, thenFuncName, isUnless: false));

        // Extract "Else" Block (if exists) to a new Function
        if (node.ElseBlock != null) {
            string elseFuncName = $"__if_else_{_context.FunctionCodeGen.Next(8)}";
            List<IRInstruction> elseInsts = node.ElseBlock.Accept(this) ?? [];
            RegisterFunction(elseFuncName, elseInsts);

            // if (cond != 1) call elseFunc, implemented as 'unless cond == 1'
            insts.Add(new CallIfInstruction(condition, elseFuncName, isUnless: true));
        }

        return insts;
    }

    public List<IRInstruction> VisitWhile(WhileNode node) {
        // This is instructions to be added to the parent this WHILE is in.
        var insts = new List<IRInstruction>();

        // ~~~~~~ Create Loop Function Name ~~~~~~ //
        string loopFuncName = $"__while_{_context.FunctionCodeGen.Next(8)}";

        // ~~~~~~~ Build Loop Function Body ~~~~~~ //
        var loopInsts = new List<IRInstruction>();

        // Add original body
        loopInsts.AddRange(node.Body.Accept(this) ?? []);

        // Re-evaluate condition inside the loop at the end
        var condInLoop = node.Condition.Accept(evaluator.Inst(loopInsts));
        var loopCondition = new Condition(
            ConditionType.Equal,
            condInLoop,
            new ConstantOperand("1")
        );

        // Recursive call: execute if cond == 1 run function loopFuncName
        loopInsts.Add(new CallIfInstruction(loopCondition, loopFuncName));

        // Register the loop function
        RegisterFunction(loopFuncName, loopInsts);

        // ~~~~~~~~~~ Build Caller Logic ~~~~~~~~~ //
        // Evaluate condition initially
        var condInCaller = node.Condition.Accept(evaluator.Inst(insts));
        var callerCondition = new Condition(
            ConditionType.Equal,
            condInCaller,
            new ConstantOperand("1")
        );

        // Initial call
        insts.Add(new CallIfInstruction(callerCondition, loopFuncName));

        return insts;
    }

    // --- Standard Visitors (Pass-through or Simple) ---

    public List<IRInstruction> VisitBlock(BlockNode node) {
        var insts = new List<IRInstruction>();
        foreach (var statement in node.Statements) {
            insts.AddRange(statement.Accept(this) ?? []);
        }
        return insts;
    }

    public List<IRInstruction> VisitExpressionStatement(ExpressionStatementNode node) {
        var insts = new List<IRInstruction>();
        // Evaluate the expression (instructions will be added to insts)
        // The result is discarded as this is a statement
        node.Expression.Accept(evaluator.Inst(insts));
        return insts;
    }

    public List<IRInstruction> VisitReturn(ReturnNode node) {
        // Find Function Scope
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
        // "Return 1" is standard to stop execution in MC functions
        insts.Add(new ReturnInstruction(new ConstantOperand("1")));
        return insts;
    }

    public List<IRInstruction> VisitAssignment(AssignmentNode node) {
        var insts = new List<IRInstruction>();
        var valueOperand = node.Expression.Accept(evaluator.Inst(insts));
        var variableOperand = node.Variable.Accept(evaluator);
        insts.Add(new MoveInstruction(valueOperand, variableOperand));
        return insts;
    }

    public List<IRInstruction> VisitCommand(CommandNode node) {
        return [new CommandInstruction(node.Command)];
    }

    public List<IRInstruction> VisitVariableDefinition(VariableDefinitionNode node) {
        var insts = new List<IRInstruction>();
        BuildVariableDefinition(node, insts);
        return insts;
    }

    // Unused / Empty for IR generation
    public List<IRInstruction> VisitArgument(ArgumentNode node) => [];
    public List<IRInstruction> VisitBinaryOp(BinaryOpNode node) => [];
    public List<IRInstruction> VisitFor(ForNode node) => [];
    public List<IRInstruction> VisitFunctionCall(FunctionCallNode node) => []; // Handled in Evaluator usually, or needs implementation if statement
    public List<IRInstruction> VisitIdentifier(IdentifierNode node) => [];
    public List<IRInstruction> VisitLiteral(LiteralNode node) => [];
    public List<IRInstruction> VisitModifier(ModifierNode node) => [];
    public List<IRInstruction> VisitUnaryOp(UnaryOpNode node) => [];
}