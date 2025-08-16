
using Antlr4.Runtime.Misc;
using Deco.Compiler.Data;
using System;
using System.Linq;

namespace Deco.Compiler {
    /// <summary>
    /// This visitor walks the ANTLR parse tree and populates a DataPack object
    /// using the generic data models (ResourceLocation, Tag, etc.).
    /// </summary>
    public class DecoCodeVisitor : DecoBaseVisitor<object> {
        private readonly DataPack _dataPack;

        public DecoCodeVisitor(DataPack dataPack) {
            _dataPack = dataPack;
        }

        public override object VisitFunction([NotNull] DecoParser.FunctionContext context) {
            ResourceLocation functionLocation = new ResourceLocation(Util.GenerateRandomString(8), _dataPack.MainNamespace);
            var nameModifier = context.modifier().FirstOrDefault(m => m.name.Text.ToString() == "name");

            // ====================================================== //
            // =========== Figure out the function's name =========== //
            // ====================================================== //
            if (nameModifier != null) {
                var expressions = nameModifier.expression();
                if (expressions.Length > 0 && expressions[0].STRING() != null) {
                    string nameValue = expressions[0].STRING().GetText().Trim('"');

                    functionLocation.SetLocation(nameValue);
                }
            }

            var currentFunction = _dataPack.FindOrCreateFunction(functionLocation);

            // ====================================================== //
            // =============== Handle other modifiers =============== //
            // ====================================================== //
            foreach (var modifierContext in context.modifier()) {
                string modifierName = modifierContext.name.Text.ToString() ?? "";

                switch (modifierName) {
                    case "load":
                    case "tick":
                        var tagLocation = new ResourceLocation(modifierName, "minecraft");
                        var tag = _dataPack.FindOrCreateTag(tagLocation, TagType.Function);
                        if (!tag.Values.Any(v => v.ToString() == currentFunction.Location.ToString())) {
                            tag.Values.Add(currentFunction.Location);
                        }
                        break;

                    case "tag":
                        var expressions = modifierContext.expression();
                        if (expressions.Length > 0 && expressions[0].STRING() != null) {
                            string tagValue = expressions[0].STRING().GetText().Trim('"');

                            string ns = "deco";
                            string path = tagValue;

                            if (tagValue.Contains(':')) {
                                var parts = tagValue.Split(new[] { ':' }, 2);
                                ns = parts[0];
                                path = parts[1];
                            }

                            var customTagLocation = new ResourceLocation(path, ns);
                            var customTag = _dataPack.FindOrCreateTag(customTagLocation, TagType.Function);
                            if (!customTag.Values.Any(v => v.ToString() == currentFunction.Location.ToString())) {
                                customTag.Values.Add(currentFunction.Location);
                            }
                        }
                        break;
                }
            }

            // Directly loop through all statements in the function's body
            foreach (var statement in context.statement()) {
                if (statement.COMMAND() != null) {
                    string rawCommand = statement.COMMAND().GetText();
                    if (rawCommand.StartsWith("@`") && rawCommand.EndsWith("`")) {
                        string command = rawCommand.Substring(2, rawCommand.Length - 3);
                        currentFunction.Commands.Add(command);
                    }
                }
            }

            // ====================================================== //
            // ========== Tackle with function's arguments ========== //
            // ====================================================== //
            if (context.arguments() != null) {
                if (!_dataPack.Flags.ContainsKey("deco.argument.init")) {
                    // Sets a flag when first encountering a function that needs the arguments system.  
                    // This ensures we only add initialization code to the datapack's load function once.  
                    _dataPack.Flags.Add("deco.argument.init", "true");

                    _dataPack.OnLoadFunction.PrependCommands([
                        $"scoreboard objectives remove {_dataPack.ID}",
                        $"scoreboard objectives add {_dataPack.ID} dummy",
                        $"data modify storage {_dataPack.ID} stack_int set value []",
                        $"data modify storage {_dataPack.ID} stack_float set value []",
                        $"data modify storage {_dataPack.ID} stack_string set value []",
                    ]);
                }

                // Read arguments at the beginning of function
                
            }

            return null;
        }
    }
}
