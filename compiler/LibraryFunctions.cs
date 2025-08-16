using Deco.Compiler.Data;
using System;

namespace Deco.Compiler {
    public static class LibraryFunctions {
        public static void HandlePrintFunction(DecoParser.FunctionCallContext context, DataPack dataPack, string functionName) {
            var arguments = context.expression();
            var currentMcFunction = dataPack.Functions.FindOrCreate(dataPack.Functions.Locations[functionName]);

            if (arguments.Length != 1) {
                Console.Error.WriteLine($"Error: Function 'print' expects 1 argument, but received {arguments.Length}.");
                return;
            }
            var contentArgument = arguments[0];
            string content = "";

            if (contentArgument.IDENTIFIER() != null) {
                string identifierName = contentArgument.IDENTIFIER().GetText();
                ParameterInfo info = dataPack.Functions.Table[functionName].Parameters.Find((v) => v.Name == identifierName);
                if (info == null) return;
                string trueName = info.StorageName;
                // Assuming identifiers refer to values stored in the data pack's main storage.
                // Assuming identifiers refer to values stored in the data pack's main storage.
                if (info.Type == "int") {
                    currentMcFunction.Commands.Add($"tellraw @a {{\"score\":{{\"name\":\"{trueName}\",\"objective\":\"{dataPack.ID}\"}}}}");
                } else if (info.Type == "float") {
                    currentMcFunction.Commands.Add($"tellraw @a {{\"nbt\":\"{trueName}\",\"storage\":\"{dataPack.ID}\"}}");
                } else if (info.Type == "string") {
                    currentMcFunction.Commands.Add($"tellraw @a {{\"nbt\":\"{trueName}\",\"storage\":\"{dataPack.ID}\"}}");
                } else {
                    Console.Error.WriteLine($"Error: Unsupported type '{info.Type}' for print function identifier.");
                }
            } else if (contentArgument.STRING() != null) {
                content = contentArgument.STRING().GetText();
                // Remove quotes from the content if it's a string literal
                if (content.StartsWith("\"") && content.EndsWith("\"")) {
                    content = content.Substring(1, content.Length - 2);
                }
                // Escape any inner double quotes for JSON
                content = content.Replace("\"", "\\\"");
                currentMcFunction.Commands.Add($"tellraw @a {{\"text\":\"{content}\"}}");
            } else if (contentArgument.NUMBER() != null) {
                content = contentArgument.NUMBER().GetText();
                currentMcFunction.Commands.Add($"tellraw @a {{\"text\":\"{content}\"}}");
            } else {
                Console.Error.WriteLine($"Error: 'print' function only supports string, number literals, or identifiers as arguments.");
                return;
            }
        }
    }
}
