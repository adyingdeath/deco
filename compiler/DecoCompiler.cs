using Deco.Compiler.Data;

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
                foreach (var statement in decoFunction.Context.statement()) {
                    ProcessStatement(statement, decoFunction);
                }
            }
        }

        private void ProcessStatement(DecoParser.StatementContext statement, DecoFunction decoFunction) {
            var currentMcFunction = decoFunction.McFunction;
            if (statement.COMMAND() != null) {
                string rawCommand = statement.COMMAND().GetText();
                if (rawCommand.StartsWith("@`") && rawCommand.EndsWith("`")) {
                    string command = rawCommand.Substring(2, rawCommand.Length - 3);
                    currentMcFunction.Commands.Add(command);
                }
            } else if (statement.expression()?.functionCall() != null) {
                ProcessFunctionCall(statement.expression().functionCall(), decoFunction);
            }
            // Other statement types (variable definitions, assignments, etc.) can be handled here.
        }

        private void ProcessFunctionCall(DecoParser.FunctionCallContext context, DecoFunction currentDecoFunction) {
            string functionNameToCall = context.name.Text;
            McFunction currentMcFunction = currentDecoFunction.McFunction;

            // Handle built-in library functions
            if (functionNameToCall == "print") {
                LibraryFunctions.HandlePrintFunction(context, _dataPack, currentDecoFunction);
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

            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var parameter = signature.Parameters[i];

                if (argument.NUMBER() != null) {
                    switch (parameter.Type) {
                        case "int":
                            currentMcFunction.Commands.Add($"scoreboard players set tmp_eval {_dataPack.ID} {argument.NUMBER().GetText()}");
                            currentMcFunction.Commands.Add($"execute store result storage {_dataPack.ID} tmp_val int 1 run scoreboard players get tmp_eval {_dataPack.ID}");
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append from storage {_dataPack.ID} tmp_val");
                            break;
                        case "float":
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_val set value {argument.NUMBER().GetText()}f");
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append from storage {_dataPack.ID} tmp_val");
                            break;
                        default:
                            Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}'. Expected {parameter.Type}, got NUMBER.");
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append value {{}}");
                            break;
                    }
                } else if (argument.STRING() != null) {
                    if (parameter.Type == "string") {
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append value {argument.STRING().GetText()}");
                    } else {
                        Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}'. Expected {parameter.Type}, got STRING.");
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append value {{}}");
                    }
                } else if (argument.IDENTIFIER() != null) {
                    string identifierName = argument.IDENTIFIER().GetText();
                    var passedVarInfo = currentDecoFunction.Signature.Parameters.Find(p => p.Name == identifierName);

                    if (passedVarInfo == null) {
                        Console.Error.WriteLine($"Error: Unknown identifier '{identifierName}' in function '{currentDecoFunction.Name}'.");
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append value {{}}");
                        continue;
                    }
                    if (parameter.Type != passedVarInfo.Type) {
                         Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}'. Expected {parameter.Type}, got {passedVarInfo.Type} from variable '{identifierName}'.");
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append value {{}}");
                        continue;
                    }

                    switch (passedVarInfo.Type) {
                        case "int":
                            currentMcFunction.Commands.Add($"execute store result storage {_dataPack.ID} tmp_val int 1 run scoreboard players get {passedVarInfo.StorageName} {_dataPack.ID}");
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append from storage {_dataPack.ID} tmp_val");
                            break;
                        case "float":
                        case "string":
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append from storage {_dataPack.ID} {passedVarInfo.StorageName}");
                            break;
                    }
                } else {
                    Console.Error.WriteLine($"Warning: Calling functions with non-literal arguments is not fully implemented yet.");
                    currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} call_stack append value {{}}");
                }
            }

            // Stage 2: Save the current values of the callee's parameters to the real stack.
            // This allows for recursion. These values will be restored after the function returns.
            foreach (var parameter in signature.Parameters) {
                var storageName = parameter.StorageName;
                switch (parameter.Type) {
                    case "int":
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
