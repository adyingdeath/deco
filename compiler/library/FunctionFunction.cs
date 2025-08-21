using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using System;
using System.Collections.Generic;

namespace Deco.Compiler.Library {
    public class FunctionFunction : LibraryFunction {
        public override string Name => "function";
        public override string ReturnType => "void";
        public override List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();

        public override Operand Execute(
            DecoParser.FunctionCallContext context,
            DataPack dataPack,
            DecoFunction currentDecoFunction,
            ExpressionCompiler expressionCompiler
        ) {
            var arguments = context.expression();
            if (arguments.Length != 1) {
                Console.Error.WriteLine($"Error: Function '{Name}' expects exactly 1 argument, but received {arguments.Length}.");
                return new ConstantOperand("0", "void");
            }

            var argExpression = arguments[0];

            try {
                var constantEvaluator = new ConstantEvaluator(dataPack);
                var constOperand = constantEvaluator.Evaluate(argExpression);

                if (constOperand.Type != "string") {
                    Console.Error.WriteLine($"Error: Argument for function '{Name}' must resolve to a function name or a resource location string.");
                    return new ConstantOperand("0", "void");
                }

                currentDecoFunction.McFunction.Commands.Add($"function {constOperand.Value}");
            }
            catch (Exception e) {
                Console.Error.WriteLine($"Error: Could not resolve function argument '{argExpression.GetText()}'. {e.Message}");
                return new ConstantOperand("0", "void");
            }

            return new ConstantOperand("0", "void");
        }
    }
}

