using Deco.Compiler.Data;

namespace Deco.Compiler.Modifiers {
    public abstract class FunctionModifier {
        public abstract string Name { get; }
        public abstract void Apply(DecoParser.ModifierContext context, DataPack dataPack, McFunction mcFunction);
    }
}
