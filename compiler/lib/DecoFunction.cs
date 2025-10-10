namespace Deco.Compiler.Lib;

public class DecoFunctionParameter(string type, string name) {
    public string Type = type;
    public string Name = name;
}

public abstract class DecoFunction {
    /// <summary>
    /// The function's name
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// The function's return type. String like "int", "string".
    /// Use "void" if no return value.
    /// </summary>
    public abstract string ReturnType { get; }
    /// <summary>
    /// The function's parameters type. String list. Each like "int", "string".
    /// </summary>
    public abstract List<DecoFunctionParameter> Parameters { get; }
    /// <summary>
    /// The concrete logic of this DecoFunction.
    /// </summary>
    /// <param name="context">A class with which you can insert minecraft
    /// commands and do some other operations.</param>
    /// <param name="arguments">This function's input arguments</param>
    /// <param name="returnValue">This is provided only when the function has
    /// return type</param>
    public abstract void Run(
        Context context, List<Argument> arguments, Argument? returnValue
    );
}