
namespace Deco.Compiler.Lib.Core;

public class PrintFunction : DecoFunction {
    public override string Name => "print";

    public override string ReturnType => "void";

    public override List<DecoFunctionParameter> Parameters => [new DecoFunctionParameter("int", "a")];

    public override void Run(Context context, List<Argument> arguments, Argument? returnValue) {
        return;
    }
}