using Deco.Compiler.Data;
using System.Linq;

namespace Deco.Compiler.Modifiers {
    public class LoadModifier : FunctionModifier {
        public override string Name => "load";

        public override void Apply(DecoParser.ModifierContext context, DataPack dataPack, McFunction mcFunction) {
            var tagLocation = new ResourceLocation("load", "minecraft");
            var tag = dataPack.FindOrCreateTag(tagLocation, TagType.Function);
            if (!tag.Values.Any(v => v.ToString() == mcFunction.Location.ToString())) {
                tag.Values.Add(mcFunction.Location);
            }
        }
    }
}
