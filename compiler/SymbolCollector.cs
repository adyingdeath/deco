using Antlr4.Runtime.Misc;
using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using Deco.Compiler.Modifiers;
using static Deco.Compiler.CompilerConstants;
using Deco.Compiler.Library;
using Deco.Compiler.Library.Types;
using Deco.Compiler.Core;

namespace Deco.Compiler {
    /// <summary>
    /// This visitor performs the first pass, walking the ANTLR parse tree to discover all
    /// function declarations, their metadata (modifiers, name), and populates the DataPack
    /// with function signatures, locations, and contexts for the second pass.
    /// </summary>
    public class SymbolCollector : DecoBaseVisitor<object> {
        private readonly DataPack _dataPack;
        private readonly Dictionary<string, FunctionModifier> _functionModifiers;
        private readonly LibraryRegistry _typeRegistry;
        private readonly SymbolTable _globalSymbolTable;

        public SymbolTable GlobalSymbolTable => _globalSymbolTable;

        public SymbolCollector(DataPack dataPack) {
            _dataPack = dataPack;

            _functionModifiers = new Dictionary<string, FunctionModifier>();
            RegisterFunctionModifier([
                new LoadModifier(),
                new TickModifier(),
                new TagModifier(),
            ]);

            // Initialize type registry for symbol collection phase
            _typeRegistry = new LibraryRegistry();
            var coreLibrary = new CoreLibrary();
            coreLibrary.Register(_typeRegistry);

            _globalSymbolTable = new SymbolTable();
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
                ReturnType = _typeRegistry.GetType(context.type.Text) ?? _typeRegistry.GetType("void")
            };

            var argsContext = context.arguments();
            if (argsContext != null) {
                foreach (var arg in argsContext.argument()) {
                    string storageName = _dataPack.Functions.ParameterIdCounter.ToString("x");
                    _dataPack.Functions.ParameterIdCounter++;

                    IDecoType paramType = _typeRegistry.GetType("int"); // Default
                    var parsedType = _typeRegistry.GetType(arg.type.Text);
                    if (parsedType != null) {
                        paramType = parsedType;
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

            // Add function symbol to global symbol table
            var functionSymbol = new Symbol(functionName, new FunctionType(), functionName);
            if (!_globalSymbolTable.Add(functionSymbol)) {
                Console.Error.WriteLine($"Error: Function '{functionName}' already exists in global symbol table.");
            }

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
