namespace Deco.Compiler.Lib.Api;

/// <summary>
/// Marks a class as a Deco Library.
/// The compiler will scan classes with this attribute for library functions.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DecoLibraryAttribute(string namespaceName) : Attribute {
    public string Namespace { get; } = namespaceName;
}

/// <summary>
/// Marks a static method as a Deco Function.
/// The method signature should match: 
/// static [Operand|void] Name(LibraryContext ctx, [DecoArgument] Operand arg1, ...)
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DecoFunctionAttribute(string name, string returnType = "void") : Attribute {
    public string Name { get; } = name;
    public string ReturnType { get; set; } = returnType;
}

/// <summary>
/// Specifies the Deco type for a function parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class DecoArgumentAttribute(string type) : Attribute {
    public string Type { get; } = type;
}