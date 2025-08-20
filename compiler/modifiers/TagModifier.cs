using Deco.Compiler.Data;
using System;
using System.Linq;

namespace Deco.Compiler.Modifiers {
    public class TagModifier : FunctionModifier {
        public override string Name => "tag";

        public override void Apply(DecoParser.ModifierContext context, DataPack dataPack, McFunction mcFunction) {
            var expressions = context.expression();
            if (expressions.Length > 0) {
                var primary = Util.GetPrimaryContext(expressions[0]);
                if (primary?.STRING() != null) {
                    string tagValue = primary.STRING().GetText().Trim('"');
                    var customTagLocation = ResourceLocation.Parse(tagValue, dataPack.MainNamespace);
                    var customTag = dataPack.FindOrCreateTag(customTagLocation, TagType.Function);
                    if (!customTag.Values.Any(v => v.ToString() == mcFunction.Location.ToString())) {
                        customTag.Values.Add(mcFunction.Location);
                    }
                }
            }
        }
    }
}
