
using Antlr4.Runtime.Misc;
using Deco.Compiler.Data;

namespace Deco.Compiler
{
    /// <summary>
    /// This visitor walks the ANTLR parse tree and populates a DataPack object
    /// using the generic data models (ResourceLocation, Tag, etc.).
    /// </summary>
    public class DecoCodeVisitor : DecoBaseVisitor<object>
    {
        private readonly DataPack _dataPack;

        public DecoCodeVisitor(DataPack dataPack)
        {
            _dataPack = dataPack;
        }

        public override object VisitFunction([NotNull] DecoParser.FunctionContext context)
        {
            var functionLocation = new ResourceLocation(context.name.Text, "deco");
            var currentFunction = _dataPack.FindOrCreateFunction(functionLocation);

            // Handle function type to add to standard tags (load, tick)
            string functionType = context.type.Text;
            if (functionType == "load" || functionType == "tick")
            {
                var tagLocation = new ResourceLocation(functionType, "minecraft");
                var tag = _dataPack.FindOrCreateTag(tagLocation, TagType.Functions);
                tag.Values.Add(currentFunction.Location);
            }

            // Directly loop through all statements in the function's body
            foreach (var statement in context.statement())
            {
                if (statement.COMMAND() != null)
                {
                    string rawCommand = statement.COMMAND().GetText();
                    if (rawCommand.StartsWith("@`") && rawCommand.EndsWith("`"))
                    {
                        string command = rawCommand.Substring(2, rawCommand.Length - 3);
                        currentFunction.Commands.Add(command);
                    }
                }
            }

            return null;
        }
    }
}
