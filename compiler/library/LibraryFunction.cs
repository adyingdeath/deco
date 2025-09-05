using Deco.Compiler.Data;
using Deco.Compiler.Expressions;
using System.Collections.Generic;

namespace Deco.Compiler.Library {
    public abstract class LibraryFunction {
        public abstract string Name { get; }
        public abstract string ReturnType { get; }
        public abstract List<ParameterInfo> Parameters { get; }

        public abstract Operand Execute(
            DecoParser.FunctionCallContext context,
            DataPack dataPack,
            DecoFunction currentDecoFunction,
            ExpressionCompiler expressionCompiler
        );
    }
}