using Antlr4.Runtime.Misc;
using Deco.Compiler.Data;
using System.Collections.Generic;
using System.Linq;

namespace Deco.Compiler
{
    /// <summary>
    /// This visitor performs the first pass, walking the ANTLR parse tree to discover all
    /// function declarations, their metadata (modifiers, name), and populates the DataPack
    /// with function signatures, locations, and contexts for the second pass.
    /// </summary>
    public class SymbolCollector : DecoBaseVisitor<object>
    {
        private readonly DataPack _dataPack;

        public SymbolCollector(DataPack dataPack)
        {
            _dataPack = dataPack;
        }

        public override object VisitFunction([NotNull] DecoParser.FunctionContext context)
        {
            string functionName = context.name.Text;

            if (_dataPack.Functions.Table.ContainsKey(functionName))
            {
                // Optionally, log a warning here about a duplicate function definition.
                return null;
            }

            // 1. Discover signature
            var signature = new FunctionSignature
            {
                ReturnType = context.type.Text
            };

            var argsContext = context.arguments();
            if (argsContext != null)
            {
                foreach (var arg in argsContext.argument())
                {
                    string storageName = _dataPack.Functions.ParameterIdCounter.ToString("x");
                    _dataPack.Functions.ParameterIdCounter++;
                    
                    signature.Parameters.Add(new ParameterInfo
                    {
                        Type = arg.type.Text,
                        Name = arg.name.Text,
                        StorageName = storageName
                    });
                }
            }
            _dataPack.Functions.Table.Add(functionName, signature);

            // 2. Determine ResourceLocation
            ResourceLocation functionLocation = new ResourceLocation(Util.GenerateRandomString(8), _dataPack.MainNamespace);
            var nameModifier = context.modifier().FirstOrDefault(m => m.name.Text == "name");
            if (nameModifier != null)
            {
                var expressions = nameModifier.expression();
                if (expressions.Length > 0 && expressions[0].STRING() != null)
                {
                    string nameValue = expressions[0].STRING().GetText().Trim('"');
                    functionLocation.SetLocation(nameValue);
                }
            }

            // 3. Create McFunction and store mappings
            var currentFunction = _dataPack.Functions.FindOrCreate(functionLocation);
            _dataPack.Functions.Locations.Add(functionName, functionLocation);
            _dataPack.Functions.Contexts.Add(functionName, context);

            // 4. Handle other modifiers
            foreach (var modifierContext in context.modifier())
            {
                string modifierName = modifierContext.name.Text;
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
                            var customTagLocation = new ResourceLocation(tagValue);
                            var customTag = _dataPack.FindOrCreateTag(customTagLocation, TagType.Function);
                            if (!customTag.Values.Any(v => v.ToString() == currentFunction.Location.ToString()))
                            {
                                customTag.Values.Add(currentFunction.Location);
                            }
                        }
                        break;
                }
            }
            
            // 5. Handle argument system initialization
            if (context.arguments() != null)
            {
                if (!_dataPack.Flags.ContainsKey("deco.argument.init"))
                {
                    _dataPack.Flags.Add("deco.argument.init", "true");
                    _dataPack.Functions.OnLoadFunction.PrependCommands([
                        $"scoreboard objectives remove {_dataPack.ID}",
                        $"scoreboard objectives add {_dataPack.ID} dummy",
                        $"data modify storage {_dataPack.ID} stack_int set value []",
                        $"data modify storage {_dataPack.ID} stack_float set value []",
                        $"data modify storage {_dataPack.ID} stack_string set value []",
                    ]);
                }
            }

            // Do not visit children; the second pass handles function bodies.
            return null;
        }
    }
}