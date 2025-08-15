
using Antlr4.Runtime.Misc;
using Deco.Compiler.Data;
using System;
using System.Linq;

namespace Deco.Compiler
{
    /// <summary>
    /// This visitor walks the ANTLR parse tree and populates a DataPack object
    /// using the generic data models (ResourceLocation, Tag, etc.).
    /// </summary>
    public class DecoCodeVisitor : DecoBaseVisitor<object>
    {
        private readonly DataPack _dataPack;
        private static readonly Random _random = new Random();

        public DecoCodeVisitor(DataPack dataPack)
        {
            _dataPack = dataPack;
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public override object VisitFunction([NotNull] DecoParser.FunctionContext context)
        {
            ResourceLocation functionLocation = new ResourceLocation(GenerateRandomString(8));
            var nameModifier = context.modifier().FirstOrDefault(m => m.name.Text.ToString() == "name");

            if (nameModifier != null)
            {
                var expressions = nameModifier.expression();
                if (expressions.Length > 0 && expressions[0].STRING() != null)
                {
                    string nameValue = expressions[0].STRING().GetText().Trim('"');

                    functionLocation.SetLocation(nameValue);
                }
            }

            Console.Out.WriteLine(" >>>>>>>>>>>>> " + functionLocation);

            var currentFunction = _dataPack.FindOrCreateFunction(functionLocation);

            // Second pass on modifiers to handle tags and other properties
            foreach (var modifierContext in context.modifier())
            {
                string modifierName = modifierContext.name.Text.ToString() ?? "";

                switch (modifierName)
                {
                    case "load":
                    case "tick":
                        var tagLocation = new ResourceLocation(modifierName, "minecraft");
                        var tag = _dataPack.FindOrCreateTag(tagLocation, TagType.Function);
                        if (!tag.Values.Any(v => v.ToString() == currentFunction.Location.ToString()))
                        {
                            tag.Values.Add(currentFunction.Location);
                        }
                        break;

                    case "tag":
                        var expressions = modifierContext.expression();
                        if (expressions.Length > 0 && expressions[0].STRING() != null)
                        {
                            string tagValue = expressions[0].STRING().GetText().Trim('"');

                            string ns = "deco";
                            string path = tagValue;

                            if (tagValue.Contains(':'))
                            {
                                var parts = tagValue.Split(new[] { ':' }, 2);
                                ns = parts[0];
                                path = parts[1];
                            }

                            var customTagLocation = new ResourceLocation(path, ns);
                            var customTag = _dataPack.FindOrCreateTag(customTagLocation, TagType.Function);
                            if (!customTag.Values.Any(v => v.ToString() == currentFunction.Location.ToString()))
                            {
                                customTag.Values.Add(currentFunction.Location);
                            }
                        }
                        break;
                }
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
