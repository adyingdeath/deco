using Deco.Compiler.Data;
using Deco.Compiler.Expressions; // New import
using System.Text.Json.Nodes;
using System; // For Console.Error.WriteLine

namespace Deco.Compiler {
    public static class LibraryFunctions {
        public static void HandlePrintFunction(DecoParser.FunctionCallContext context, DataPack dataPack, DecoFunction currentDecoFunction, ExpressionCompiler expressionCompiler) {
            var arguments = context.expression();
            var currentMcFunction = currentDecoFunction.McFunction;

            if (arguments.Length == 0) {
                Console.Error.WriteLine("Error: Function 'print' expects at least 1 argument.");
                return;
            }

            var jsonArray = new JsonArray();

            for (int i = 0; i < arguments.Length; i++) {
                var argument = arguments[i];

                var evaluatedArg = expressionCompiler.Evaluate(argument);
                JsonObject component = null;

                switch (evaluatedArg.Type) {
                    case "bool":
                        string tempStringStorage = expressionCompiler.GetNextTemp();
                        currentMcFunction.Commands.Add($"data modify storage {dataPack.ID} {tempStringStorage} set value \"false\"");
                        currentMcFunction.Commands.Add($"execute if score {evaluatedArg.StorageName} {dataPack.ID} matches 1 run data modify storage {dataPack.ID} {tempStringStorage} set value \"true\"");
                        component = new JsonObject {
                            ["nbt"] = tempStringStorage,
                            ["storage"] = dataPack.ID
                        };
                        break;
                    case "int":
                        component = new JsonObject {
                            ["score"] = new JsonObject {
                                ["name"] = evaluatedArg.StorageName,
                                ["objective"] = dataPack.ID
                            }
                        };
                        break;
                    case "float":
                    case "string":
                        component = new JsonObject {
                            ["nbt"] = evaluatedArg.StorageName,
                            ["storage"] = dataPack.ID
                        };
                        break;
                    default:
                        Console.Error.WriteLine($"Error: Unsupported type '{evaluatedArg.Type}' for print function argument.");
                        return;
                }

                if (component != null) {
                    jsonArray.Add(component);
                }

                if (i < arguments.Length - 1) {
                    jsonArray.Add(new JsonObject { ["text"] = "  " });
                }
            }

            string finalJson = jsonArray.ToJsonString();
            currentMcFunction.Commands.Add($"tellraw @a {finalJson}");
        }
    }
}
