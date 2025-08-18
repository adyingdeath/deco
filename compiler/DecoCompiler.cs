using Deco.Compiler.Data;
using Deco.Compiler.Expressions; // New import
using System; // For Console.Error.WriteLine

namespace Deco.Compiler {
    /// <summary>
    /// This visitor performs the second pass. It iterates through the functions discovered
    /// in the first pass and generates the Minecraft commands for each statement in their bodies.
    /// </summary>
    public class DecoCompiler {
        private readonly DataPack _dataPack;

        public DecoCompiler(DataPack dataPack) {
            _dataPack = dataPack;
        }

        public void GenerateCode() {
            foreach (var decoFunction in _dataPack.Functions.DecoFunctions.Values) {
                // Process each statement in the function body
                foreach (var statement in decoFunction.Context.block().statement()) {
                    ProcessStatement(statement, decoFunction);
                }
            }
        }

        private void ProcessStatement(DecoParser.StatementContext statement, DecoFunction decoFunction) {
            var expressionCompiler = new ExpressionCompiler(decoFunction, _dataPack, decoFunction.SymbolTable);

            if (statement.COMMAND() != null) {
                string rawCommand = statement.COMMAND().GetText();
                if (rawCommand.StartsWith("@`") && rawCommand.EndsWith('`')) {
                    string command = rawCommand[2..^1];
                    decoFunction.McFunction.Commands.Add(command);
                }
            } else if (statement.variableDefinition() != null) {
                var varDef = statement.variableDefinition();
                string varName = varDef.name.Text;
                string varTypeString = varDef.type.Text;

                string varType = "int"; // Default
                if (varTypeString == "int" || varTypeString == "float" || varTypeString == "string" || varTypeString == "bool") {
                    varType = varTypeString;
                } else {
                    Console.Error.WriteLine($"Warning: Unknown variable type '{varTypeString}' for variable '{varName}'. Defaulting to int.");
                }

                // Generate a unique storage name for the variable
                string storageName = _dataPack.Functions.ParameterIdCounter.ToString("x");
                _dataPack.Functions.ParameterIdCounter++;

                var newSymbol = new Symbol(varName, varType, storageName);
                if (!decoFunction.SymbolTable.Add(newSymbol)) {
                    Console.Error.WriteLine($"Error: Variable '{varName}' already defined in function '{decoFunction.Name}'.");
                }
                // Initialize with default value (0 for int/float, empty string for string)
                switch (varType) {
                    case "int":
                    case "bool":
                        decoFunction.McFunction.Commands.Add($"scoreboard players set {newSymbol.StorageName} {_dataPack.ID} 0");
                        break;
                    case "float":
                        decoFunction.McFunction.Commands.Add($"data modify storage {_dataPack.ID} {newSymbol.StorageName} set value 0.0f");
                        break;
                    case "string":
                        decoFunction.McFunction.Commands.Add($"data modify storage {_dataPack.ID} {newSymbol.StorageName} set value \"\"");
                        break;
                }
            } else if (statement.assignment() != null) {
                var assignment = statement.assignment();
                string varName = assignment.IDENTIFIER().GetText();
                var targetSymbol = decoFunction.SymbolTable.Get(varName);

                if (targetSymbol == null) {
                    Console.Error.WriteLine($"Error: Attempt to assign to undefined variable '{varName}' in function '{decoFunction.Name}'.");
                    return;
                }

                var evaluatedExpression = expressionCompiler.Evaluate(assignment.expression());

                // Assign the result of the expression to the variable
                switch (targetSymbol.Type) {
                    case "int":
                    case "bool":
                        if (targetSymbol.Type != evaluatedExpression.Type) {
                            Console.Error.WriteLine($"Error: Type mismatch for assignment to '{varName}'. Expected {targetSymbol.Type}, got {evaluatedExpression.Type}.");
                            return;
                        }
                        decoFunction.McFunction.Commands.Add($"scoreboard players operation {targetSymbol.StorageName} {_dataPack.ID} = {evaluatedExpression.StorageName} {_dataPack.ID}");
                        break;
                    case "float":
                    case "string":
                        if (targetSymbol.Type != evaluatedExpression.Type) {
                            Console.Error.WriteLine($"Error: Type mismatch for assignment to '{varName}'. Expected {targetSymbol.Type}, got {evaluatedExpression.Type}.");
                            return;
                        }
                        decoFunction.McFunction.Commands.Add($"data modify storage {_dataPack.ID} {targetSymbol.StorageName} set from storage {_dataPack.ID} {evaluatedExpression.StorageName}");
                        break;
                }
                targetSymbol.IsInitialized = true;
            } else if (statement.expression() != null) {
                // Evaluate the expression, but discard the result if it's not an assignment or function call
                // For now, only function calls are explicitly handled here.
                var primaryContext = Util.GetPrimaryContext(statement.expression());
                if (primaryContext?.functionCall() != null) {
                    ProcessFunctionCall(primaryContext.functionCall(), decoFunction);
                } else {
                    // Evaluate other expressions for side effects, if any.
                    // For now, just evaluate and ignore the result.
                    expressionCompiler.Evaluate(statement.expression());
                }
            } else if (statement.if_statement() != null) {
                ProcessIfStatement(statement.if_statement(), decoFunction);
            }
            // Other statement types (return) can be handled here.
        }

        private void ProcessIfStatement(DecoParser.If_statementContext context, DecoFunction currentFunction) {
            var parentSymbolTable = currentFunction.SymbolTable;
            var expressionCompiler = new ExpressionCompiler(currentFunction, _dataPack, parentSymbolTable);

            // 1. Evaluate condition
            var condition = expressionCompiler.Evaluate(context.expression());
            if (condition.Type != "bool") {
                Console.Error.WriteLine("Error: if statement condition must be a boolean expression.");
                return;
            }

            var ifBlock = context.block(0);
            var ifBodyStatements = ifBlock.statement();

            var elseIfNode = context.if_statement();
            var elseBlockNode = context.block().Length > 1 ? context.block(1) : null;

            bool hasElse = elseIfNode != null || elseBlockNode != null;

            // Optimization for single command with no else
            if (ifBodyStatements.Length == 1 && !hasElse && ifBodyStatements[0].COMMAND() != null) {
                var singleStatement = ifBodyStatements[0];
                string rawCommand = singleStatement.COMMAND().GetText();
                if (rawCommand.StartsWith("@`") && rawCommand.EndsWith('`')) {
                    string command = rawCommand[2..^1];
                    currentFunction.McFunction.Commands.Add($"execute if score {condition.StorageName} {_dataPack.ID} matches 1 run {command}");
                    return;
                }
            }

            var ifMcFunction = CreateFunctionForBlock(ifBodyStatements, currentFunction, parentSymbolTable);
            currentFunction.McFunction.Commands.Add($"execute if score {condition.StorageName} {_dataPack.ID} matches 1 run function {ifMcFunction.Location}");

            if (hasElse) {
                var randomCode = Util.GenerateRandomString(8);
                var elseMcFunction = new McFunction(new ResourceLocation(randomCode, _dataPack.MainNamespace));
                _dataPack.Functions.McFunctions.Add(elseMcFunction);

                var elseSymbolTable = new SymbolTable(parentSymbolTable);
                var elseDecoFunction = new DecoFunction($"{currentFunction.Name}_{randomCode}", currentFunction.Signature, elseMcFunction, currentFunction.Context, elseSymbolTable);

                if (elseIfNode != null) {
                    ProcessIfStatement(elseIfNode, elseDecoFunction);
                } else if (elseBlockNode != null) {
                    // elseBlockNode must be non-null
                    var elseBodyStatements = elseBlockNode.statement();
                    foreach (var statement in elseBodyStatements) {
                        ProcessStatement(statement, elseDecoFunction);
                    }
                }

                currentFunction.McFunction.Commands.Add($"execute if score {condition.StorageName} {_dataPack.ID} matches 0 run function {elseMcFunction.Location}");
            }
        }

        private McFunction CreateFunctionForBlock(DecoParser.StatementContext[] statements, DecoFunction parentFunction, SymbolTable parentSymbolTable) {
            var randomCode = Util.GenerateRandomString(8);
            var mcFunction = new McFunction(new ResourceLocation(randomCode, _dataPack.MainNamespace));
            _dataPack.Functions.McFunctions.Add(mcFunction);

            var blockSymbolTable = new SymbolTable(parentSymbolTable);
            var blockDecoFunction = new DecoFunction(
                $"{parentFunction.Name}_{randomCode}",
                parentFunction.Signature,
                mcFunction,
                parentFunction.Context,
                blockSymbolTable
            );

            foreach (var statement in statements) {
                ProcessStatement(statement, blockDecoFunction);
            }

            return mcFunction;
        }

        private void ProcessFunctionCall(DecoParser.FunctionCallContext context, DecoFunction currentDecoFunction) {
            string functionNameToCall = context.name.Text;
            McFunction currentMcFunction = currentDecoFunction.McFunction;
            var expressionCompiler = new ExpressionCompiler(currentDecoFunction, _dataPack, currentDecoFunction.SymbolTable);

            // Handle built-in library functions
            if (functionNameToCall == "print") {
                LibraryFunctions.HandlePrintFunction(context, _dataPack, currentDecoFunction, expressionCompiler); // Pass expressionCompiler
                return; // Handled, so return
            }

            // 1. Look up function
            if (!_dataPack.Functions.DecoFunctions.TryGetValue(functionNameToCall, out var calledDecoFunction)) {
                // Error: calling an undefined function
                Console.Error.WriteLine($"Error: Attempt to call undefined function '{functionNameToCall}'.");
                return;
            }
            var signature = calledDecoFunction.Signature;
            var locationToCall = calledDecoFunction.McFunction.Location;

            var arguments = context.expression();

            // 2. Check argument count
            if (arguments.Length != signature.Parameters.Count) {
                Console.Error.WriteLine($"Error: Function '{functionNameToCall}' expects {signature.Parameters.Count} arguments, but received {arguments.Length}.");
                return;
            }

            // --- Function Call Implementation ---
            // Stage 1: Evaluate all arguments and push them to a temporary list ('call_stack').
            // This resolves all argument values *before* any parameters are modified, fixing bugs
            // like `foo(b, a)` where `a` would be overwritten before being passed.
            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack set value []");

            for (int i = 0; i < arguments.Length; i++) {
                var argument = arguments[i];
                var parameter = signature.Parameters[i];

                var evaluatedArg = expressionCompiler.Evaluate(argument);

                if (parameter.Type != evaluatedArg.Type) {
                    Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}'. Expected {parameter.Type}, got {evaluatedArg.Type}.");
                    currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append value {{}}"); // Append dummy value
                    continue;
                }

                switch (evaluatedArg.Type) {
                    case "int":
                    case "bool":
                        currentMcFunction.Commands.Add($"execute store result storage {_dataPack.ID} tmp_val int 1 run scoreboard players get {evaluatedArg.StorageName} {_dataPack.ID}");
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append from storage {_dataPack.ID} tmp_val");
                        break;
                    case "float":
                    case "string":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append from storage {_dataPack.ID} {evaluatedArg.StorageName}");
                        break;
                }
            }

            // Stage 2: Save the current values of the callee's parameters to the real stack.
            // This allows for recursion. These values will be restored after the function returns.
            foreach (var parameter in signature.Parameters) {
                var storageName = parameter.StorageName;
                switch (parameter.Type) {
                    case "int":
                    case "bool":
                        currentMcFunction.Commands.Add($"execute store result storage {_dataPack.ID} tmp_arg int 1 run scoreboard players get {storageName} {_dataPack.ID}");
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} stack_int prepend from storage {_dataPack.ID} tmp_arg");
                        break;
                    case "float":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} stack_float prepend from storage {_dataPack.ID} {storageName}");
                        break;
                    case "string":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} stack_string prepend from storage {_dataPack.ID} {storageName}");
                        break;
                }
            }

            // Stage 3: Assign the evaluated arguments from 'call_stack' to the actual parameter storage.
            for (int i = 0; i < signature.Parameters.Count; i++) {
                var parameter = signature.Parameters[i];
                var storageName = parameter.StorageName;

                switch (parameter.Type) {
                    case "int":
                    case "bool":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_val set from storage {_dataPack.ID} call_stack[{i}]");
                        currentMcFunction.Commands.Add($"execute store result score {storageName} {_dataPack.ID} run data get storage {_dataPack.ID} tmp_val 1");
                        break;
                    case "float":
                    case "string":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {storageName} set from storage {_dataPack.ID} call_stack[{i}]");
                        break;
                }
            }

            // Cleanup temporary evaluation storage
            //currentMcFunction.Commands.Add($"data remove storage {_dataPack.ID} call_stack");
            //currentMcFunction.Commands.Add($"data remove storage {_dataPack.ID} tmp_val");

            // Stage 4: Call the function
            currentMcFunction.Commands.Add($"function {locationToCall}");

            // Stage 5: Restore the context. Pop values from the real stack back into parameter storage.
            for (int i = signature.Parameters.Count - 1; i >= 0; i--) {
                var parameter = signature.Parameters[i];
                var storageName = parameter.StorageName;
                switch (parameter.Type) {
                    case "int":
                    case "bool":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_arg set from storage {_dataPack.ID} stack_int[0]");
                        currentMcFunction.Commands.Add($"execute store result score {storageName} {_dataPack.ID} run data get storage {_dataPack.ID} tmp_arg 1");
                        currentMcFunction.Commands.Add($"data remove storage {_dataPack.ID} stack_int[0]");
                        break;
                    case "float":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {storageName} set from storage {_dataPack.ID} stack_float[0]");
                        currentMcFunction.Commands.Add($"data remove storage {_dataPack.ID} stack_float[0]");
                        break;
                    case "string":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {storageName} set from storage {_dataPack.ID} stack_string[0]");
                        currentMcFunction.Commands.Add($"data remove storage {_dataPack.ID} stack_string[0]");
                        break;
                }
            }
        }
    }
}