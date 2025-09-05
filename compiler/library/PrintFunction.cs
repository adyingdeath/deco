using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using System.Text.Json.Nodes;

namespace Deco.Compiler.Library {
    public class PrintFunction : LibraryFunction {
        public override string Name => "print";
        public override string ReturnType => "void";
        public override List<ParameterInfo> Parameters => new List<ParameterInfo>(); // Print can take any number of arguments, so this will be empty for now. We'll handle argument count check in Execute.

        public override Operand Execute(
            DecoParser.FunctionCallContext context,
            DataPack dataPack,
            DecoFunction currentDecoFunction,
            ExpressionCompiler expressionCompiler
        ) {
            var arguments = context.expression();
            var currentMcFunction = currentDecoFunction.McFunction;

            if (arguments.Length == 0) {
                Console.Error.WriteLine("Error: Function 'print' expects at least 1 argument.");
                return new ConstantOperand("0", "void");
            }

            var jsonArray = new JsonArray();

            for (int i = 0; i < arguments.Length; i++) {
                var argument = arguments[i];

                var evaluatedArg = expressionCompiler.Evaluate(argument);
                JsonObject component = null;

                switch (evaluatedArg.Type) {
                    case "bool":
                        string tempStringStorage = expressionCompiler.GetNextTemp();
                        currentMcFunction.Commands.Add($"data modify storage {dataPack.ID} {tempStringStorage} set value \"false\"");
                        currentMcFunction.Commands.Add($"execute if score {evaluatedArg.StorageName} {dataPack.ID} matches 1 run data modify storage {dataPack.ID} {tempStringStorage} set value \"true\"");
                        component = new JsonObject {
                            ["nbt"] = tempStringStorage,
                            ["storage"] = dataPack.ID
                        };
                        break;
                    case "int":
                        component = new JsonObject {
                            ["score"] = new JsonObject {
                                ["name"] = evaluatedArg.StorageName,
                                ["objective"] = dataPack.ID
                            }
                        };
                        break;
                    case "float":
                    case "string":
                        component = new JsonObject {
                            ["nbt"] = evaluatedArg.StorageName,
                            ["storage"] = dataPack.ID
                        };
                        break;
                    default:
                        Console.Error.WriteLine($"Error: Unsupported type '{evaluatedArg.Type}' for print function argument.");
                        return new ConstantOperand("0", "void");
                }

                if (component != null) {
                    jsonArray.Add(component);
                }

                if (i < arguments.Length - 1) {
                    jsonArray.Add(new JsonObject { ["text"] = "  " });
                }
            }

            string finalJson = jsonArray.ToJsonString();
            currentMcFunction.Commands.Add($"tellraw @a {finalJson}");
            return new ConstantOperand("0", "void");
        }
    }
}