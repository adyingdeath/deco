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
            // A unified, frame-based calling convention.

            // Stage 1: Evaluate all arguments into a temporary frame object.
            // This resolves all argument values *before* any parameters are modified.
            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_frame set value {{ints:[], floats:[], strings:[]}}");
            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var parameter = signature.Parameters[i]; // Used for type context

                if (argument.NUMBER() != null)
                {
                    switch (parameter.Type)
                    {
                        case "int":
                            currentMcFunction.Commands.Add($"scoreboard players set tmp_eval {_dataPack.ID} {argument.NUMBER().GetText()}");
                            currentMcFunction.Commands.Add($"execute store result storage {_dataPack.ID} tmp_frame.ints[-1] int 1 run scoreboard players get tmp_eval {_dataPack.ID}");
                            break;
                        case "float":
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_val set value {argument.NUMBER().GetText()}f");
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_frame.floats append from storage {_dataPack.ID} tmp_val");
                            break;
                        default:
                            Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}'. Expected {parameter.Type}, got NUMBER.");
                            break;
                    }
                }
                else if (argument.STRING() != null)
                {
                    if (parameter.Type == "string")
                    {
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_val set value {argument.STRING().GetText()}");
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_frame.strings append from storage {_dataPack.ID} tmp_val");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}'. Expected {parameter.Type}, got STRING.");
                    }
                }
                else if (argument.IDENTIFIER() != null)
                {
                    string identifierName = argument.IDENTIFIER().GetText();
                    var passedVarInfo = currentDecoFunction.Signature.Parameters.Find(p => p.Name == identifierName);

                    if (passedVarInfo == null)
                    {
                        Console.Error.WriteLine($"Error: Unknown identifier '{identifierName}' in function '{currentDecoFunction.Name}'.");
                        continue;
                    }
                    if (parameter.Type != passedVarInfo.Type)
                    {
                        Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}'. Expected {parameter.Type}, got {passedVarInfo.Type} from variable '{identifierName}'.");
                        continue;
                    }

                    switch (passedVarInfo.Type)
                    {
                        case "int":
                            currentMcFunction.Commands.Add($"execute store result storage {_dataPack.ID} tmp_frame.ints[-1] int 1 run scoreboard players get {passedVarInfo.StorageName} {_dataPack.ID}");
                            break;
                        case "float":
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_frame.floats append from storage {_dataPack.ID} {passedVarInfo.StorageName}");
                            break;
                        case "string":
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} tmp_frame.strings append from storage {_dataPack.ID} {passedVarInfo.StorageName}");
                            break;
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Warning: Calling functions with non-literal arguments is not fully implemented yet.");
                }
            }

            // Stage 2: Save the current context by pushing a new frame onto the system_stack.
            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} system_stack prepend value {{ints:[], floats:[], strings:[]}}");
            var intParams = signature.Parameters.Where(p => p.Type == "int").ToList();
            var floatParams = signature.Parameters.Where(p => p.Type == "float").ToList();
            var stringParams = signature.Parameters.Where(p => p.Type == "string").ToList();

            foreach (var p in intParams) {
                currentMcFunction.Commands.Add($"execute store result storage {_dataPack.ID} system_stack[0].ints[-1] int 1 run scoreboard players get {p.StorageName} {_dataPack.ID}");
            }
            foreach (var p in floatParams) {
                currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} system_stack[0].floats append from storage {_dataPack.ID} {p.StorageName}");
            }
            foreach (var p in stringParams) {
                currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} system_stack[0].strings append from storage {_dataPack.ID} {p.StorageName}");
            }

            // Stage 3: Assign the evaluated arguments from tmp_frame to the actual parameter storage.
            var intArgIndex = 0;
            var floatArgIndex = 0;
            var stringArgIndex = 0;
            for (int i = 0; i < signature.Parameters.Count; i++)
            {
                var p = signature.Parameters[i];
                switch (p.Type)
                {
                    case "int":
                        currentMcFunction.Commands.Add($"execute store result score {p.StorageName} {_dataPack.ID} run data get storage {_dataPack.ID} tmp_frame.ints[{intArgIndex++}] 1");
                        break;
                    case "float":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {p.StorageName} set from storage {_dataPack.ID} tmp_frame.floats[{floatArgIndex++}]");
                        break;
                    case "string":
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {p.StorageName} set from storage {_dataPack.ID} tmp_frame.strings[{stringArgIndex++}]");
                        break;
                }
            }
            currentMcFunction.Commands.Add($"data remove storage {_dataPack.ID} tmp_frame");

            // Stage 4: Call the function.
            currentMcFunction.Commands.Add($"function {locationToCall}");

            // Stage 5: Restore the context by popping the frame from the system_stack.
            intArgIndex = 0;
            floatArgIndex = 0;
            stringArgIndex = 0;
            foreach (var p in intParams) {
                currentMcFunction.Commands.Add($"execute store result score {p.StorageName} {_dataPack.ID} run data get storage {_dataPack.ID} system_stack[0].ints[{intArgIndex++}] 1");
            }
            foreach (var p in floatParams) {
                currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {p.StorageName} set from storage {_dataPack.ID} system_stack[0].floats[{floatArgIndex++}]");
            }
            foreach (var p in stringParams) {
                currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {p.StorageName} set from storage {_dataPack.ID} system_stack[0].strings[{stringArgIndex++}]");
            }
            currentMcFunction.Commands.Add($"data remove storage {_dataPack.ID} system_stack[0]");
        }
    }
}
