using Antlr4.Runtime.Misc;
using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using Deco.Compiler.Modifiers;
using System.Linq;
using System;
using System.Collections.Generic;
using static Deco.Compiler.CompilerConstants;

namespace Deco.Compiler {
    /// <summary>
    /// This visitor performs the first pass, walking the ANTLR parse tree to discover all
    /// function declarations, their metadata (modifiers, name), and populates the DataPack
    /// with function signatures, locations, and contexts for the second pass.
    /// </summary>
    public class SymbolCollector : DecoBaseVisitor<object> {
        private readonly DataPack _dataPack;
        private readonly Dictionary<string, FunctionModifier> _functionModifiers;

        public SymbolCollector(DataPack dataPack) {
            _dataPack = dataPack;

            _functionModifiers = new Dictionary<string, FunctionModifier>();
            RegisterFunctionModifier([
                new LoadModifier(),
                new TickModifier(),
                new TagModifier(),
            ]);
        }

        private void RegisterFunctionModifier(FunctionModifier[] modifiers) {
            foreach (var modifier in modifiers) {
                _functionModifiers.Add(modifier.Name, modifier);
            }
        }

        public override object VisitFunction([NotNull] DecoParser.FunctionContext context) {
            string functionName = context.name.Text;

            if (_dataPack.Functions.DecoFunctions.ContainsKey(functionName)) {
                // Optionally, log a warning here about a duplicate function definition.
                return null;
            }

            // Create a new SymbolTable for the function
            var functionSymbolTable = new SymbolTable(); // No parent for top-level functions

            // 1. Discover signature
            var signature = new FunctionSignature {
                ReturnType = context.type.Text
            };

            var argsContext = context.arguments();
            if (argsContext != null) {
                foreach (var arg in argsContext.argument()) {
                    string storageName = _dataPack.Functions.ParameterIdCounter.ToString("x");
                    _dataPack.Functions.ParameterIdCounter++;

                    string paramType = "int"; // Default
                    if (arg.type.Text == "int" || arg.type.Text == "float" || arg.type.Text == "string" || arg.type.Text == "bool") {
                        paramType = arg.type.Text;
                    } else {
                        Console.Error.WriteLine($"Warning: Unknown parameter type '{arg.type.Text}' for parameter '{arg.name.Text}' in function '{functionName}'. Defaulting to int.");
                    }

                    var paramInfo = new ParameterInfo(arg.name.Text, paramType, storageName);
                    signature.Parameters.Add(paramInfo);
                    functionSymbolTable.Add(paramInfo); // Add parameter to symbol table
                }
            }

            // 2. Determine ResourceLocation
            ResourceLocation? functionLocation = null;
            var nameModifierContext = context.modifier().FirstOrDefault(m => m.name.Text == "name");
            if (nameModifierContext != null && nameModifierContext.expression().Length > 0) {
                var primary = Util.GetPrimaryContext(nameModifierContext.expression()[0]);
                if (primary?.STRING() != null) {
                    string nameValue = primary.STRING().GetText().Trim('"');
                    functionLocation = ResourceLocation.Parse(nameValue, _dataPack.MainNamespace);
                }
            }

            if (functionLocation == null) {
                functionLocation = new ResourceLocation(Util.GenerateRandomString(8), _dataPack.MainNamespace);
            }

            // 3. Create McFunction and store mappings
            var mcFunction = _dataPack.Functions.FindOrCreateMcFunction(functionLocation);
            var decoFunction = new DecoFunction(functionName, signature, mcFunction, context, functionSymbolTable);
            _dataPack.Functions.DecoFunctions.Add(functionName, decoFunction);

            // 4. Handle other modifiers
            foreach (var modifierContext in context.modifier()) {
                string modifierName = modifierContext.name.Text;
                if (_functionModifiers.TryGetValue(modifierName, out var modifier)) {
                    modifier.Apply(modifierContext, _dataPack, mcFunction);
                }
            }

            // 5. Handle argument and return system initialization
            if (!_dataPack.Flags.ContainsKey("deco.system.init")) {
                _dataPack.Flags.Add("deco.system.init", "true");
                _dataPack.Functions.OnLoadFunction.PrependCommands([
                    // Argument system
                    $"scoreboard objectives remove {_dataPack.ID}",
                    $"scoreboard objectives add {_dataPack.ID} dummy",
                    $"data modify storage {_dataPack.ID} stack_int set value []",
                    $"data modify storage {_dataPack.ID} stack_float set value []",
                    $"data modify storage {_dataPack.ID} stack_string set value []",
                    // Return system
                    $"scoreboard objectives add {ReturnFlagObjective} dummy",
                    $"scoreboard players set {ReturnFlagPlayer} {ReturnFlagObjective} 0"
                ]);
            }

            // Do not visit children; the second pass handles function bodies.
            return null;
        }
    }
}
