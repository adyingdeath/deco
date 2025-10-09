namespace Deco.Compiler.Lib;

public abstract class DecoFunction(string def) {
    /// <summary>
    /// The definition string of this function, whose format is the same as normal
    /// funtions defined in a deco source file. For example:
    /// <code>
    /// int rectangle(int width, int height) {}
    /// </code>
    /// </summary>
    public string Definition = def;
    /// <summary>
    /// The concrete logic of this DecoFunction.
    /// </summary>
    /// <param name="context">A class with which you can insert minecraft
    /// commands and do some other operations.</param>
    /// <param name="arguments">This function's input arguments</param>
    /// <param name="returnValue">This is provided only when the function has
    /// return type</param>
    public abstract void Run(Context context, List<Argument> arguments, Argument? returnValue);
}