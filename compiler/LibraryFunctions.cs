using Deco.Compiler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Deco.Compiler {
    public static class LibraryFunctions {
        public static void HandlePrintFunction(DecoParser.FunctionCallContext context, DataPack dataPack, string functionName) {
            var arguments = context.expression();
            var currentMcFunction = dataPack.Functions.FindOrCreate(dataPack.Functions.Locations[functionName]);

            if (arguments.Length == 0) {
                Console.Error.WriteLine("Error: Function 'print' expects at least 1 argument.");
                return;
            }

            var jsonArray = new JsonArray();

            for (int i = 0; i < arguments.Length; i++) {
                var argument = arguments[i];
                JsonObject component = null;

                if (argument.IDENTIFIER() != null) {
                    string identifierName = argument.IDENTIFIER().GetText();

                    if (!dataPack.Functions.Table.TryGetValue(functionName, out var containingFunctionSignature)) {
                        Console.Error.WriteLine($"Internal Error: Could not find signature for function '{functionName}'.");
                        return;
                    }

                    ParameterInfo info = containingFunctionSignature.Parameters.Find(p => p.Name == identifierName);

                    if (info == null) {
                        Console.Error.WriteLine($"Error: Unknown identifier '{identifierName}' in function '{functionName}'. Only function parameters are currently supported in 'print'.");
                        return;
                    }

                    switch (info.Type) {
                        case "int":
                            component = new JsonObject {
                                ["score"] = new JsonObject {
                                    ["name"] = info.StorageName,
                                    ["objective"] = dataPack.ID
                                }
                            };
                            break;
                        case "float":
                        case "string":
                            component = new JsonObject {
                                ["nbt"] = info.StorageName,
                                ["storage"] = dataPack.ID
                            };
                            break;
                        default:
                            Console.Error.WriteLine($"Error: Unsupported type '{info.Type}' for print function identifier '{identifierName}'.");
                            return;
                    }
                } else if (argument.STRING() != null) {
                    string content = argument.STRING().GetText();
                    if (content.StartsWith('"') && content.EndsWith('"')) {
                        content = content[1..^1];
                    }
                    component = new JsonObject { ["text"] = content };
                } else if (argument.NUMBER() != null) {
                    string content = argument.NUMBER().GetText();
                    component = new JsonObject { ["text"] = content };
                } else {
                    Console.Error.WriteLine("Error: unsupported type for 'print'.");
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
