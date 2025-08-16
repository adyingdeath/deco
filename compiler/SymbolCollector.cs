using Antlr4.Runtime.Misc;
using Deco.Compiler.Data;
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

            if (_dataPack.Functions.DecoFunctions.ContainsKey(functionName))
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

            // 2. Determine ResourceLocation
            ResourceLocation functionLocation;
            var nameModifier = context.modifier().FirstOrDefault(m => m.name.Text == "name");
            if (nameModifier != null && nameModifier.expression().Length > 0 && nameModifier.expression()[0].STRING() != null)
            {
                string nameValue = nameModifier.expression()[0].STRING().GetText().Trim('"');
                functionLocation = ResourceLocation.Parse(nameValue, _dataPack.MainNamespace);
            }
            else
            {
                functionLocation = new ResourceLocation(Util.GenerateRandomString(8), _dataPack.MainNamespace);
            }

            // 3. Create McFunction and store mappings
            var mcFunction = _dataPack.Functions.FindOrCreateMcFunction(functionLocation);
            var decoFunction = new DecoFunction(functionName, signature, mcFunction, context);
            _dataPack.Functions.DecoFunctions.Add(functionName, decoFunction);

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
                        if (!tag.Values.Any(v => v.ToString() == mcFunction.Location.ToString()))
                        {
                            tag.Values.Add(mcFunction.Location);
                        }
                        break;

                    case "tag":
                        var expressions = modifierContext.expression();
                        if (expressions.Length > 0 && expressions[0].STRING() != null)
                        {
                            string tagValue = expressions[0].STRING().GetText().Trim('"');
                            var customTagLocation = ResourceLocation.Parse(tagValue, _dataPack.MainNamespace);
                            var customTag = _dataPack.FindOrCreateTag(customTagLocation, TagType.Function);
                            if (!customTag.Values.Any(v => v.ToString() == mcFunction.Location.ToString()))
                            {
                                customTag.Values.Add(mcFunction.Location);
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
                        $"data modify storage {_dataPack.ID} system_stack set value []",
                    ]);
                }
            }

            // Do not visit children; the second pass handles function bodies.
            return null;
        }
    }
}
