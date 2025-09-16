namespace Deco.Types;

/// <summary>
/// Abstract base class for all types in the Deco language.
/// </summary>
public abstract class Type;

/// <summary>
/// Represents primitive types like int, bool, string, void.
/// </summary>
public class PrimitiveType(string name) : Type {
    public string Name { get; } = name;

    public override string ToString() => Name;
}

/// <summary>
/// Represents function types with return type and parameter types.
/// </summary>
public class FunctionType(Type returnType, List<Type> parameterTypes) : Type {
    public Type ReturnType { get; } = returnType;
    public List<Type> ParameterTypes { get; } = parameterTypes ?? [];

    public override string ToString() => $"{ReturnType}({string.Join(", ", ParameterTypes)})";
}

public class UnresolvedType(string name) : Type {
    public string Name { get; } = name;
    public override string ToString() => Name;
}

/// <summary>
/// Utility class for type operations and predefined types.
/// </summary>
public static class TypeUtils {
    public static readonly PrimitiveType IntType = new("int");
    public static readonly PrimitiveType BoolType = new("bool");
    public static readonly PrimitiveType StringType = new("string");
    public static readonly PrimitiveType VoidType = new("void");

    /// <summary>
    /// Parses a type from a string name. For primitives, returns the predefined instance.
    /// For unknown types, creates a new PrimitiveType.
    /// </summary>
    public static Type ParseType(string typeName) {
        return typeName switch {
            "int" => IntType,
            "bool" => BoolType,
            "string" => StringType,
            "void" => VoidType,
            _ => new UnresolvedType(typeName)
        };
    }
}
