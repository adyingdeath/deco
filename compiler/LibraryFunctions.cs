using Deco.Compiler.Data;
using System;

namespace Deco.Compiler
{
    public static class LibraryFunctions
    {
        public static void HandlePrintFunction(DecoParser.FunctionCallContext context, McFunction currentMcFunction, DataPack dataPack)
        {
            var arguments = context.expression();
            if (arguments.Length != 1)
            {
                Console.Error.WriteLine($"Error: Function 'print' expects 1 argument, but received {arguments.Length}.");
                return;
            }
            var contentArgument = arguments[0];
            string content = "";

            if (contentArgument.STRING() != null)
            {
                content = contentArgument.STRING().GetText();
                // Remove quotes from the content if it's a string literal
                if (content.StartsWith(""") && content.EndsWith("""))
                {
                    content = content.Substring(1, content.Length - 2);
                }
            }
            else if (contentArgument.NUMBER() != null)
            {
                content = contentArgument.NUMBER().GetText();
            }
            else
            {
                Console.Error.WriteLine($"Error: 'print' function only supports string or number literals as arguments.");
                return;
            }
            
            // Escape any special characters if necessary for Minecraft commands
            // For now, a simple /say command will suffice.
            currentMcFunction.Commands.Add($"say {content}");
        }
    }
}