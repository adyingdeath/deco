
using Antlr4.Runtime.Misc;
using Deco.Compiler.Data;

namespace Deco.Compiler
{
    /// <summary>
    /// This visitor walks the ANTLR parse tree and populates a DataPack object
    /// with the functions and commands found in the source code.
    /// The logic is self-contained within VisitFunction for clarity.
    /// </summary>
    public class DecoCodeVisitor : DecoBaseVisitor<object>
    {
        private readonly DataPack _dataPack;

        public DecoCodeVisitor(DataPack dataPack)
        {
            _dataPack = dataPack;
        }

        /// <summary>
        /// Visits a function node, creating a new McFunction in the DataPack.
        /// It directly iterates over all statements within the function body,
        /// making the traversal logic explicit and self-contained.
        /// </summary>
        public override object VisitFunction([NotNull] DecoParser.FunctionContext context)
        {
            string functionName = context.name.Text;
            var currentFunction = _dataPack.AddFunction(functionName);

            // Directly loop through all statements in the function's body
            foreach (var statement in context.statement())
            {
                // Check if the statement is a raw command
                if (statement.COMMAND() != null)
                {
                    string rawCommand = statement.COMMAND().GetText();
                    if (rawCommand.StartsWith("@`") && rawCommand.EndsWith("`"))
                    {
                        string command = rawCommand.Substring(2, rawCommand.Length - 3);
                        currentFunction.Commands.Add(command);
                    }
                }
                // In the future, you could add else-if blocks here to handle
                // other types of statements like variable assignments, function calls, etc.
            }

            // We return null because we are not building a result tree, just populating the DataPack.
            // Note that we do NOT call base.VisitFunction(context) as we handled the traversal ourselves.
            return null;
        }
    }
}
