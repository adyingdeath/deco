using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using Deco.Compiler.Library.Functions;
using Deco.Compiler.Library.Types;
using System.Text.Json.Nodes;

namespace Deco.Compiler.Core.Functions {
    public class PrintFunction : IDecoFunction {
        public string Name => "print";

        public FunctionSignature Signature => new ExtendedFunctionSignature {
            ReturnType = new VoidType(),
            IsVariadic = true
        }.ToLegacySignature();

        public Operand Execute(LibContext context, List<Operand> arguments) {
            if (arguments.Count == 0) {
                Console.Error.WriteLine("Error: Function 'print' expects at least 1 argument.");
                return new ConstantOperand("0", "void");
            }

            var jsonArray = new JsonArray();

            for (int i = 0; i < arguments.Count; i++) {
                var argument = arguments[i];
                JsonObject component;

                string argType = GetOperandType(argument);
                string storageName = GetOperandStorageName(argument);

                switch (argType) {
                    case "bool":
                        string tempStringStorage = context.GetNextTemp();
                        context.CurrentMcFunction.Commands.Add($"data modify storage {context.DataPack.ID} {tempStringStorage} set value \"false\"");
                        context.CurrentMcFunction.Commands.Add($"execute if score {storageName} {context.DataPack.ID} matches 1 run data modify storage {context.DataPack.ID} {tempStringStorage} set value \"true\"");
                        component = new JsonObject {
                            ["nbt"] = tempStringStorage,
                            ["storage"] = context.DataPack.ID
                        };
                        break;
                    case "int":
                        component = new JsonObject {
                            ["score"] = new JsonObject {
                                ["name"] = storageName,
                                ["objective"] = context.DataPack.ID
                            }
                        };
                        break;
                    case "float":
                    case "string":
                        component = new JsonObject {
                            ["nbt"] = storageName,
                            ["storage"] = context.DataPack.ID
                        };
                        break;
                    default:
                        Console.Error.WriteLine($"Error: Unsupported type '{argType}' for print function argument.");
                        return new ConstantOperand("0", "void");
                }

                if (component != null) {
                    jsonArray.Add(component);
                }

                if (i < arguments.Count - 1) {
                    jsonArray.Add(new JsonObject { ["text"] = "  " });
                }
            }

            string finalJson = jsonArray.ToJsonString();
            context.CurrentMcFunction.Commands.Add($"tellraw @a {finalJson}");
            return new ConstantOperand("0", "void");
        }

        private string GetOperandType(Operand operand) {
            if (operand is ConstantOperand constantOp) {
                return constantOp.Type;
            } else if (operand is SymbolOperand symbolOp) {
                return symbolOp.Symbol.Type;
            }
            throw new System.NotSupportedException($"Unsupported operand type: {operand.GetType()}");
        }

        private string GetOperandStorageName(Operand operand) {
            if (operand is ConstantOperand constantOp) {
                return constantOp.Value;
            } else if (operand is SymbolOperand symbolOp) {
                return symbolOp.Symbol.StorageName;
            }
            throw new System.NotSupportedException($"Unsupported operand type: {operand.GetType()}");
        }
    }
}
