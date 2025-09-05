using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using Deco.Compiler.Library.Functions;
using Deco.Compiler.Library.Types;
using System.Text.Json.Nodes;

namespace Deco.Compiler.Core.Functions {
    public class FunctionFunction : IDecoFunction {
        public string Name => "function";

        public FunctionSignature Signature => new FunctionSignature {
            ReturnType = new VoidType(),
        };

        public Operand Execute(LibContext context, List<Operand> arguments) {
            if (arguments.Count != 1) {
                Console.Error.WriteLine($"Error: Function '{Name}' expects exactly 1 argument, but received {arguments.Count}.");
                return new ConstantOperand("0", "void");
            }

            // Get the first argument
            var arg = arguments[0];
            if (arg is not ConstantOperand constArg || constArg.Type != "string") {
                Console.Error.WriteLine($"Error: Argument for function '{Name}' must be a string constant representing a function name or resource location.");
                return new ConstantOperand("0", "void");
            }

            context.CurrentMcFunction.Commands.Add($"function {constArg.Value}");
            return new ConstantOperand("0", "void");
        }
    }
}
