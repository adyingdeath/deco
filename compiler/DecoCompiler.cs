using Deco.Compiler.Data;

namespace Deco.Compiler
{
    /// <summary>
    /// This visitor performs the second pass. It iterates through the functions discovered
    /// in the first pass and generates the Minecraft commands for each statement in their bodies.
    /// </summary>
    public class DecoCompiler
    {
        private readonly DataPack _dataPack;

        public DecoCompiler(DataPack dataPack)
        {
            _dataPack = dataPack;
        }

        public void GenerateCode()
        {
            foreach (var functionName in _dataPack.Functions.Contexts.Keys)
            {
                var context = _dataPack.Functions.Contexts[functionName];
                var functionLocation = _dataPack.Functions.Locations[functionName];
                var currentMcFunction = _dataPack.Functions.FindOrCreate(functionLocation);

                // Process each statement in the function body
                foreach (var statement in context.statement())
                {
                    ProcessStatement(statement, currentMcFunction);
                }
            }
        }

        private void ProcessStatement(DecoParser.StatementContext statement, McFunction currentMcFunction)
        {
            if (statement.COMMAND() != null)
            {
                string rawCommand = statement.COMMAND().GetText();
                if (rawCommand.StartsWith("@`") && rawCommand.EndsWith("`"))
                {
                    string command = rawCommand.Substring(2, rawCommand.Length - 3);
                    currentMcFunction.Commands.Add(command);
                }
            }
            else if (statement.expression()?.functionCall() != null)
            {
                ProcessFunctionCall(statement.expression().functionCall(), currentMcFunction);
            }
            // Other statement types (variable definitions, assignments, etc.) can be handled here.
        }

        private void ProcessFunctionCall(DecoParser.FunctionCallContext context, McFunction currentMcFunction)
        {
            string functionName = context.name.Text;
            
            // Handle built-in library functions
            if (functionName == "print")
            {
                LibraryFunctions.HandlePrintFunction(context, currentMcFunction, _dataPack);
                return; // Handled, so return
            }

            // 1. Look up function signature and location
            if (!_dataPack.Functions.Table.TryGetValue(functionName, out var signature) ||
                !_dataPack.Functions.Locations.TryGetValue(functionName, out var locationToCall)) {
                // Error: calling an undefined function
                Console.Error.WriteLine($"Error: Attempt to call undefined function '{functionName}'.");
                return;
            }

            var arguments = context.expression();

            // 2. Check argument count
            if (arguments.Length != signature.Parameters.Count)
            {
                Console.Error.WriteLine($"Error: Function '{functionName}' expects {signature.Parameters.Count} arguments, but received {arguments.Length}.");
                return;
            }

            // 3. Pass arguments directly using scoreboards or data storage
            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var parameter = signature.Parameters[i];
                var storageName = parameter.StorageName;

                // For now, we only handle literal strings and numbers
                if (argument.NUMBER() != null)
                {
                    switch (parameter.Type)
                    {
                        case "int":
                            currentMcFunction.Commands.Add($"scoreboard players set {storageName} {_dataPack.ID} {argument.NUMBER().GetText()}");
                            break;
                        case "float":
                            // Note: Minecraft scoreboards don't support floats. Storing in data is a common workaround.
                            // A scaling factor is often used if scoreboard math is needed.
                            currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {storageName} set value {argument.NUMBER().GetText()}f");
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
                        currentMcFunction.Commands.Add($"data modify storage {_dataPack.ID} {storageName} set value {argument.STRING().GetText()}");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Error: Type mismatch for parameter '{parameter.Name}'. Expected {parameter.Type}, got STRING.");
                    }
                }
                else
                {
                    // Passing variables or complex expressions would be handled here
                    Console.Error.WriteLine($"Warning: Calling functions with non-literal arguments is not fully implemented yet.");
                }
            }

            // 4. Call the function
            currentMcFunction.Commands.Add($"function {locationToCall}");
        }

        
    }
}