
namespace Deco.Compiler.Lib.Core;

public class PrintFunction : DecoFunction {
    public override string Name => "print";

    public override string ReturnType => "void";

    public override List<DecoFunctionParameter> Parameters => [new DecoFunctionParameter("int", "a")];

    public override void Run(Context context, List<Argument> arguments, Argument? returnValue) {
        // /tellraw @a {"score":{"name":"<name>","objective":"<objective>"}}
        context.Command($"tellraw @a {{\"score\":{{\"name\":\"{arguments[0].Name}\",\"objective\":\"{arguments[0].Location}\"}}}}");
        return;
    }
}